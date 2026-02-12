using Microsoft.Extensions.Options;
using Poxiao.Lab.CalcWorker.Services;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Enums;
using Poxiao.Lab.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SqlSugar;
using System.Text;
using System.Text.Json;

namespace Poxiao.Lab.CalcWorker.Worker;

/// <summary>
/// 判定任务消费者 — 监听 lab.judge.task 队列。
/// 支持两种消费模式（均为 per-item，每条消息处理一条）：
///   1. MAGNETIC_JUDGE：按炉号查找中间数据 → 更新磁性字段 → 执行判定
///   2. JUDGE：按 IntermediateDataId 查实体 → 执行判定，进度按 BatchId 汇总
/// </summary>
public class JudgeTaskConsumer : BackgroundService
{
    private readonly ILogger<JudgeTaskConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CalcProgressPublisher _progressPublisher;
    private readonly BatchProgressTracker _progressTracker;
    private readonly RabbitMqOptions _rabbitOptions;

    private IConnection? _connection;
    private IModel? _channel;

    public JudgeTaskConsumer(
        ILogger<JudgeTaskConsumer> logger,
        IServiceProvider serviceProvider,
        CalcProgressPublisher progressPublisher,
        BatchProgressTracker progressTracker,
        IOptions<RabbitMqOptions> rabbitOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _progressPublisher = progressPublisher;
        _progressTracker = progressTracker;
        _rabbitOptions = rabbitOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 等待一小段时间让 Host 完全启动
        await Task.Delay(1000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeLoop(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Judge] RabbitMQ 消费异常，5 秒后重连...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ConsumeLoop(CancellationToken stoppingToken)
    {
        var maxConcurrency = _rabbitOptions.MaxConcurrency;
        var prefetchCount = (ushort)Math.Max(_rabbitOptions.PrefetchCount, maxConcurrency);

        var factory = new ConnectionFactory
        {
            HostName = _rabbitOptions.HostName,
            Port = _rabbitOptions.Port,
            UserName = _rabbitOptions.UserName,
            Password = _rabbitOptions.Password,
            DispatchConsumersAsync = true,
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        _connection = connection;
        _channel = channel;

        // 声明判定队列
        channel.QueueDeclare(
            queue: _rabbitOptions.JudgeQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

        _logger.LogInformation(
            "[Judge] 已连接 RabbitMQ [{Host}:{Port}]，开始消费判定队列: {Queue}, 并发={Concurrency}",
            _rabbitOptions.HostName,
            _rabbitOptions.Port,
            _rabbitOptions.JudgeQueueName,
            maxConcurrency);

        var semaphore = new SemaphoreSlim(maxConcurrency);
        var channelLock = new object();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            await semaphore.WaitAsync(stoppingToken);
            _ = ProcessAndAck(ea, channel, channelLock, semaphore, stoppingToken);
        };

        channel.BasicConsume(
            queue: _rabbitOptions.JudgeQueueName,
            autoAck: false,
            consumer: consumer);

        // 保持运行直到取消
        var tcs = new TaskCompletionSource<bool>();
        stoppingToken.Register(() => tcs.TrySetResult(true));
        await tcs.Task;
    }

    private async Task ProcessAndAck(
        BasicDeliverEventArgs ea,
        IModel channel,
        object channelLock,
        SemaphoreSlim semaphore,
        CancellationToken stoppingToken)
    {
        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var taskMessage = JsonSerializer.Deserialize<CalcTaskMessage>(body);

            if (taskMessage == null)
            {
                _logger.LogWarning("[Judge] 无法反序列化消息，跳过");
                return;
            }

            _logger.LogInformation(
                "[Judge] 收到消息: TaskType={TaskType}, BatchId={BatchId}, IntermediateDataId={Id}",
                taskMessage.TaskType, taskMessage.BatchId, taskMessage.IntermediateDataId ?? "(batch)");

            if (taskMessage.TaskType == "MAGNETIC_JUDGE"
                && !string.IsNullOrEmpty(taskMessage.IntermediateDataId))
            {
                await ProcessMagneticJudgeItem(taskMessage, stoppingToken);
            }
            else if (taskMessage.TaskType == "JUDGE"
                && !string.IsNullOrEmpty(taskMessage.IntermediateDataId))
            {
                await ProcessJudgeItem(taskMessage, stoppingToken);
            }
            else
            {
                _logger.LogWarning(
                    "[Judge] 未知的判定任务类型: TaskType={TaskType}, BatchId={BatchId}",
                    taskMessage.TaskType, taskMessage.BatchId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Judge] 处理判定任务消息失败");
        }
        finally
        {
            lock (channelLock)
            {
                try
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Judge] BasicAck 失败");
                }
            }

            semaphore.Release();
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  per-item MAGNETIC_JUDGE 模式
    // ═════════════════════════════════════════════════════════════════════════

    private async Task ProcessMagneticJudgeItem(CalcTaskMessage task, CancellationToken stoppingToken)
    {
        var batchId = task.BatchId;
        var furnaceNo = task.IntermediateDataId!;
        var progressInterval = _rabbitOptions.ProgressInterval;

        _logger.LogInformation(
            "[Judge] MAGNETIC_JUDGE 开始: FurnaceNo={FurnaceNo}, BatchId={BatchId}",
            furnaceNo, batchId);

        var batchState = _progressTracker.GetOrCreate(task);
        bool success = false;

        using var scope = _serviceProvider.CreateScope();

        try
        {
            var sqlClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

            // 1. 反序列化磁性数据载荷
            if (string.IsNullOrWhiteSpace(task.MagneticDataJson))
            {
                _logger.LogWarning("[Judge] MAGNETIC_JUDGE 消息缺少 MagneticDataJson: FurnaceNo={FurnaceNo}", furnaceNo);
                return;
            }

            var payload = JsonSerializer.Deserialize<MagneticDataPayload>(task.MagneticDataJson);
            if (payload == null)
            {
                _logger.LogWarning("[Judge] MAGNETIC_JUDGE MagneticDataJson 反序列化失败: FurnaceNo={FurnaceNo}", furnaceNo);
                return;
            }

            _logger.LogInformation(
                "[Judge] 磁性载荷: FurnaceNo={FurnaceNo}, IsScratched={IsScratched}, SsPower={SsPower}, PsLoss={PsLoss}, Hc={Hc}",
                furnaceNo, payload.IsScratched, payload.SsPower, payload.PsLoss, payload.Hc);

            // 2. 通过炉号查找中间数据记录（直接取实体，供更新与判定共用）
            var entity = await sqlClient.Queryable<IntermediateDataEntity>()
                .Where(t => t.DeleteMark == null)
                .Where(t => t.FurnaceNoFormatted != null && t.FurnaceNoFormatted.Equals(furnaceNo))
                .FirstAsync();

            if (entity == null)
            {
                _logger.LogWarning(
                    "[Judge] 未找到炉号对应的中间数据: FurnaceNo={FurnaceNo}, OriginalFurnaceNo={OriginalFurnaceNo}",
                    furnaceNo, payload.OriginalFurnaceNo);
                return;
            }

            _logger.LogInformation(
                "[Judge] 找到中间数据: FurnaceNo={FurnaceNo} → Id={Id}",
                furnaceNo, entity.Id);

            // 3. 局部更新磁性字段（DB）
            var now = DateTime.Now;
            var updateChain = sqlClient.Updateable<IntermediateDataEntity>()
                .SetColumns(e => e.LastModifyUserId, payload.EditorId)
                .SetColumns(e => e.LastModifyTime, now)
                .SetColumns(e => e.PerfEditorId, payload.EditorId)
                .SetColumns(e => e.PerfEditorName, payload.EditorName)
                .SetColumns(e => e.PerfEditTime, now);

            if (payload.IsScratched)
            {
                updateChain = updateChain
                    .SetColumns(e => e.PerfAfterSsPower, payload.SsPower)
                    .SetColumns(e => e.PerfAfterPsLoss, payload.PsLoss)
                    .SetColumns(e => e.PerfAfterHc, payload.Hc)
                    .SetColumns(e => e.IsScratched, 1);

                _logger.LogInformation(
                    "[Judge] 更新刻痕后磁性字段: Id={Id}, AfterSsPower={SsPower}, AfterPsLoss={PsLoss}, AfterHc={Hc}",
                    entity.Id, payload.SsPower, payload.PsLoss, payload.Hc);
            }
            else
            {
                updateChain = updateChain
                    .SetColumns(e => e.PerfSsPower, payload.SsPower)
                    .SetColumns(e => e.PerfPsLoss, payload.PsLoss)
                    .SetColumns(e => e.PerfHc, payload.Hc)
                    .SetColumns(e => e.IsScratched, 0);

                _logger.LogInformation(
                    "[Judge] 更新正常磁性字段: Id={Id}, SsPower={SsPower}, PsLoss={PsLoss}, Hc={Hc}",
                    entity.Id, payload.SsPower, payload.PsLoss, payload.Hc);
            }

            if (payload.DetectionTime.HasValue)
            {
                updateChain = updateChain
                    .SetColumns(e => e.DetectionDate, payload.DetectionTime.Value.Date);
            }

            var updateRows = await updateChain
                .Where(e => e.Id == entity.Id)
                .ExecuteCommandAsync();

            _logger.LogInformation("[Judge] 磁性字段更新完成: Id={Id}, 影响行数={Rows}", entity.Id, updateRows);

            // 3.5 记录操作日志
            var changeSummary = payload.IsScratched
                ? $"刻痕后: SsPower={payload.SsPower}, PsLoss={payload.PsLoss}, Hc={payload.Hc}"
                : $"SsPower={payload.SsPower}, PsLoss={payload.PsLoss}, Hc={payload.Hc}";
            await InsertOpLogAsync(
                sqlClient,
                task.UserId,
                $"磁性数据导入更新：炉号 {furnaceNo}，{changeSummary}",
                "中间数据",
                entity.Id,
                changeSummary,
                furnaceNo);

            // 3.6 将磁性更新结果同步到内存实体，供判定使用（避免再次查库）
            if (payload.IsScratched)
            {
                entity.PerfAfterSsPower = payload.SsPower;
                entity.PerfAfterPsLoss = payload.PsLoss;
                entity.PerfAfterHc = payload.Hc;
                entity.IsScratched = 1;
            }
            else
            {
                entity.PerfSsPower = payload.SsPower;
                entity.PerfPsLoss = payload.PsLoss;
                entity.PerfHc = payload.Hc;
                entity.IsScratched = 0;
            }

            if (payload.DetectionTime.HasValue)
                entity.DetectionDate = payload.DetectionTime.Value.Date;
            entity.PerfEditorId = payload.EditorId;
            entity.PerfEditorName = payload.EditorName;
            entity.PerfEditTime = now;
            entity.LastModifyUserId = payload.EditorId;
            entity.LastModifyTime = now;

            // 4. 执行判定（直接使用上方实体，已含磁性更新）
            _logger.LogInformation("[Judge] 开始执行判定: Id={Id}", entity.Id);

            var calculator = scope.ServiceProvider.GetRequiredService<IntermediateDataFormulaBatchCalculator>();
            calculator.CreatorUserId = task.UserId;
            var judgeResult = await calculator.JudgeByIdAsync(entity);

            success = judgeResult.FailedCount == 0;

            _logger.LogInformation(
                "[Judge] MAGNETIC_JUDGE 判定完成: FurnaceNo={FurnaceNo}, Id={Id}, 结果={Result}, 消息={Message}",
                furnaceNo, entity.Id,
                success ? "成功" : "失败",
                judgeResult.Message);

            if (judgeResult.Errors?.Count > 0)
            {
                foreach (var err in judgeResult.Errors)
                {
                    _logger.LogWarning(
                        "[Judge] 判定错误: Id={Id}, FurnaceNo={FurnaceNo}, Error={Error}, Detail={Detail}",
                        err.IntermediateDataId, err.FurnaceNo, err.ErrorMessage, err.ErrorDetail);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[Judge] MAGNETIC_JUDGE 异常: FurnaceNo={FurnaceNo}, BatchId={BatchId}",
                furnaceNo, batchId);
        }
        finally
        {
            // 5. 报告进度
            var completed = _progressTracker.ReportCompleted(batchId, success);
            var state = _progressTracker.GetState(batchId);

            if (state != null && (completed % progressInterval == 0 || completed >= state.Total))
            {
                PublishProgressFromState(state, "PROCESSING",
                    $"磁性更新+判定: {completed}/{state.Total} (成功 {Volatile.Read(ref state.Success)}, 失败 {Volatile.Read(ref state.Failed)})");
            }

            // 6. 最后一条完成时发送 COMPLETED
            if (_progressTracker.TryMarkAllCompleted(batchId))
            {
                var totalFailed = Volatile.Read(ref state!.Failed);
                var totalSuccess = Volatile.Read(ref state.Success);

                _logger.LogInformation(
                    "[Judge] 批次 {BatchId} 磁性更新+判定全部完成: 成功={Success}, 失败={Failed}",
                    batchId, totalSuccess, totalFailed);

                var finalStatus = totalFailed > 0 && totalSuccess == 0 ? "FAILED" : "COMPLETED";
                PublishProgressFromState(state, finalStatus,
                    $"磁性导入完成: 成功 {totalSuccess} 条" +
                    (totalFailed > 0 ? $", 失败 {totalFailed} 条" : ""));

                _progressTracker.Remove(batchId);
            }
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  per-item JUDGE 模式（每条消息一条 ID，进度按批次汇总）
    // ═════════════════════════════════════════════════════════════════════════

    private async Task ProcessJudgeItem(CalcTaskMessage task, CancellationToken stoppingToken)
    {
        var batchId = task.BatchId;
        var itemId = task.IntermediateDataId!;
        var progressInterval = _rabbitOptions.ProgressInterval;

        _progressTracker.GetOrCreate(task);
        bool success = false;

        using var scope = _serviceProvider.CreateScope();

        try
        {
            var sqlClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var calculator = scope.ServiceProvider.GetRequiredService<IntermediateDataFormulaBatchCalculator>();
            calculator.CreatorUserId = task.UserId;

            var entity = await sqlClient.Queryable<IntermediateDataEntity>()
                .Where(t => t.Id == itemId && t.DeleteMark == null)
                .FirstAsync();

            if (entity == null)
            {
                _logger.LogWarning("[Judge] 未找到中间数据: Id={Id}", itemId);
                return;
            }

            var judgeResult = await calculator.JudgeByIdAsync(entity);
            success = judgeResult.FailedCount == 0;

            _logger.LogInformation(
                "[Judge] JUDGE 完成: Id={Id}, 结果={Result}",
                itemId, success ? "成功" : "失败");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Judge] JUDGE 异常: Id={Id}, BatchId={BatchId}", itemId, batchId);
        }
        finally
        {
            var completed = _progressTracker.ReportCompleted(batchId, success);
            var state = _progressTracker.GetState(batchId);

            if (state != null && (completed % progressInterval == 0 || completed >= state.Total))
            {
                PublishProgressFromState(state, "PROCESSING",
                    $"判定中: {completed}/{state.Total} (成功 {Volatile.Read(ref state.Success)}, 失败 {Volatile.Read(ref state.Failed)})");
            }

            if (_progressTracker.TryMarkAllCompleted(batchId) && state != null)
            {
                var totalFailed = Volatile.Read(ref state.Failed);
                var totalSuccess = Volatile.Read(ref state.Success);
                var finalStatus = totalFailed > 0 && totalSuccess == 0 ? "FAILED" : "COMPLETED";
                PublishProgressFromState(state, finalStatus,
                    $"判定完成: 成功 {totalSuccess} 条" + (totalFailed > 0 ? $", 失败 {totalFailed} 条" : ""));
                _progressTracker.Remove(batchId);
            }
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  公共方法
    // ═════════════════════════════════════════════════════════════════════════

    private void PublishProgressFromState(BatchState state, string status, string message)
    {
        var progress = new CalcProgressMessage
        {
            BatchId = state.BatchId,
            TenantId = state.TenantId,
            UserId = state.UserId,
            TaskType = state.TaskType,
            Total = state.Total,
            Completed = Volatile.Read(ref state.Completed),
            SuccessCount = Volatile.Read(ref state.Success),
            FailedCount = Volatile.Read(ref state.Failed),
            Status = status,
            Message = message,
            Timestamp = DateTime.Now,
        };

        _progressPublisher.Publish(progress);
    }

    private void PublishProgress(
        CalcTaskMessage task,
        int completed,
        int successCount,
        int failedCount,
        string status,
        string message)
    {
        var progress = new CalcProgressMessage
        {
            BatchId = task.BatchId ?? string.Empty,
            TenantId = task.TenantId,
            UserId = task.UserId,
            TaskType = task.TaskType,
            Total = task.TotalCount,
            Completed = completed,
            SuccessCount = successCount,
            FailedCount = failedCount,
            Status = status,
            Message = message,
            Timestamp = DateTime.Now,
        };

        _progressPublisher.Publish(progress);
    }

    /// <summary>
    /// 直接写入操作日志到 BASE_SYSLOG 表（Worker 环境无 EventBus，直接插入）。
    /// </summary>
    private async Task InsertOpLogAsync(
        ISqlSugarClient sqlClient,
        string userId,
        string abstracts,
        string moduleName = "中间数据",
        string? objectId = null,
        string? json = null,
        string? furnaceNo = null)
    {
        try
        {
            var jsonWithFurnace = string.IsNullOrEmpty(furnaceNo)
                ? json
                : (string.IsNullOrEmpty(json) ? $"炉号={furnaceNo}" : $"炉号={furnaceNo}；{json}");

            await sqlClient.Insertable(new Dictionary<string, object>
            {
                ["F_Id"] = Guid.NewGuid().ToString(),
                ["F_USERID"] = userId,
                ["F_USERNAME"] = "Worker",
                ["F_CATEGORY"] = 3, // 操作日志
                ["F_ABSTRACTS"] = abstracts,
                ["F_MODULENAME"] = moduleName,
                ["F_OBJECTID"] = objectId,
                ["F_JSON"] = jsonWithFurnace,
                ["F_CREATORTIME"] = DateTime.Now,
            }).AS("BASE_SYSLOG").ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Judge] 写入操作日志失败");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Judge] 判定 Worker 正在停止...");

        _channel?.Close();
        _connection?.Close();

        await base.StopAsync(cancellationToken);
    }
}
