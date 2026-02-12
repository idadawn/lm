using Microsoft.Extensions.Options;
using Poxiao.Lab.CalcWorker.Services;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Enums;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SqlSugar;
using System.Text;
using System.Text.Json;

namespace Poxiao.Lab.CalcWorker.Worker;

/// <summary>
/// 计算任务消费者 — 监听 lab.calc.task 队列。
/// 支持两种消费模式：
///   1. per-item 模式（IntermediateDataId 有值）：每条消息处理一条中间数据，通过 SemaphoreSlim 控制并发
///   2. batch 模式（IntermediateDataIds 有值）：手动重算/判定，保持原有逻辑
/// </summary>
public class CalcTaskConsumer : BackgroundService
{
    private readonly ILogger<CalcTaskConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly CalcProgressPublisher _progressPublisher;
    private readonly BatchProgressTracker _progressTracker;
    private readonly RabbitMqOptions _rabbitOptions;

    private IConnection? _connection;
    private IModel? _channel;

    public CalcTaskConsumer(
        ILogger<CalcTaskConsumer> logger,
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
                _logger.LogError(ex, "RabbitMQ 消费异常，5 秒后重连...");
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

        // 声明任务队列（durable 持久化）
        channel.QueueDeclare(
            queue: _rabbitOptions.TaskQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // prefetchCount = MaxConcurrency，允许多条消息同时分发
        channel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

        _logger.LogInformation(
            "已连接 RabbitMQ [{Host}:{Port}]，开始消费队列: {Queue}, 并发={Concurrency}, Prefetch={Prefetch}",
            _rabbitOptions.HostName,
            _rabbitOptions.Port,
            _rabbitOptions.TaskQueueName,
            maxConcurrency,
            prefetchCount);

        // 并发控制
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var channelLock = new object();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            await semaphore.WaitAsync(stoppingToken);

            // Fire-and-forget 处理，但保证 Ack 和 semaphore.Release
            _ = ProcessItemAndAck(ea, channel, channelLock, semaphore, stoppingToken);
        };

        channel.BasicConsume(
            queue: _rabbitOptions.TaskQueueName,
            autoAck: false,
            consumer: consumer);

        // 保持运行直到取消
        var tcs = new TaskCompletionSource<bool>();
        stoppingToken.Register(() => tcs.TrySetResult(true));
        await tcs.Task;
    }

    /// <summary>
    /// 处理单条消息并 Ack。
    /// </summary>
    private async Task ProcessItemAndAck(
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
                _logger.LogWarning("无法反序列化消息，跳过");
                return;
            }

            // ── 分流：per-item 模式 vs batch 模式（判定任务已迁移到 JudgeTaskConsumer） ──
            if (!string.IsNullOrEmpty(taskMessage.IntermediateDataId))
            {
                // per-item 模式：并发处理单条中间数据
                await ProcessPerItem(taskMessage, stoppingToken);
            }
            else if (taskMessage.IntermediateDataIds is { Count: > 0 })
            {
                // batch 模式：手动重算
                using var scope = _serviceProvider.CreateScope();
                await ProcessExistingData(scope, taskMessage);
            }
            else
            {
                // 旧 batch 模式（按 BatchId 整批处理） — 向后兼容
                using var scope = _serviceProvider.CreateScope();
                await ProcessImportTask(scope, taskMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理计算任务消息失败");
        }
        finally
        {
            // BasicAck 必须在 channel 上加锁（IModel 非线程安全）
            lock (channelLock)
            {
                try
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BasicAck 失败");
                }
            }

            semaphore.Release();
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  per-item 模式：每条消息处理一条中间数据
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// per-item 模式：处理单条中间数据的基础计算。
    /// 通过 BatchProgressTracker 追踪批次进度，最后一条完成时触发公式计算。
    /// </summary>
    private async Task ProcessPerItem(CalcTaskMessage task, CancellationToken stoppingToken)
    {
        var batchId = task.BatchId;
        var itemId = task.IntermediateDataId!;
        var progressInterval = _rabbitOptions.ProgressInterval;

        // 注册到 BatchProgressTracker（首条消息自动创建）
        var batchState = _progressTracker.GetOrCreate(task);

        bool success = false;

        using var scope = _serviceProvider.CreateScope();

        try
        {
            var intermediateRepo = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<IntermediateDataEntity>>();
            var generator = scope.ServiceProvider.GetRequiredService<IntermediateDataGenerator>();
            var sqlSugarClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

            // 1. 查询单条中间数据
            var entity = await intermediateRepo
                .AsQueryable()
                .Where(e => e.Id == itemId)
                .FirstAsync();

            if (entity == null)
            {
                _logger.LogWarning("中间数据不存在: Id={Id}, BatchId={BatchId}", itemId, batchId);
                // 视为"完成"（不再重试，避免进度卡住）
                return;
            }

            // 2. 执行基础计算
            await generator.CalculateFromEntityAsync(entity);
            entity.CalcStatus = IntermediateDataCalcStatus.SUCCESS;
            entity.CalcStatusTime = DateTime.Now;
            success = true;

            // 3. 更新 DB
            await sqlSugarClient.Updateable(entity).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "中间数据计算失败: Id={Id}, BatchId={BatchId}", itemId, batchId);

            // 标记失败并更新 DB
            try
            {
                using var failScope = _serviceProvider.CreateScope();
                var failRepo = failScope.ServiceProvider.GetRequiredService<ISqlSugarRepository<IntermediateDataEntity>>();
                var failClient = failScope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

                await failClient.Updateable<IntermediateDataEntity>()
                    .SetColumns(e => e.CalcStatus, IntermediateDataCalcStatus.FAILED)
                    .SetColumns(e => e.CalcErrorMessage, ex.Message)
                    .SetColumns(e => e.CalcStatusTime, DateTime.Now)
                    .Where(e => e.Id == itemId)
                    .ExecuteCommandAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "更新失败状态异常: Id={Id}", itemId);
            }
        }
        finally
        {
            // 4. 报告进度
            var completed = _progressTracker.ReportCompleted(batchId, success);
            var state = _progressTracker.GetState(batchId);

            // 按 progressInterval 推送进度，或最后一条
            if (state != null && (completed % progressInterval == 0 || completed >= state.Total))
            {
                PublishProgressFromState(state, "PROCESSING",
                    $"基础计算: {completed}/{state.Total} (成功 {Volatile.Read(ref state.Success)}, 失败 {Volatile.Read(ref state.Failed)})");
            }

            // 5. 如果是最后一条，触发公式计算
            if (_progressTracker.TryMarkAllCompleted(batchId))
            {
                _logger.LogInformation(
                    "批次 {BatchId} 所有基础计算完成: 成功={Success}, 失败={Failed}，开始公式计算...",
                    batchId, state?.Success, state?.Failed);

                await TriggerFormulaCalculation(batchId, stoppingToken);
            }
        }
    }

    /// <summary>
    /// 触发公式计算 + 自动判定（最后一条基础计算完成的线程调用）。
    /// </summary>
    private async Task TriggerFormulaCalculation(string batchId, CancellationToken stoppingToken)
    {
        var state = _progressTracker.GetState(batchId);
        if (state == null) return;

        try
        {
            // ── 阶段 A: 公式计算 ──
            PublishProgressFromState(state, "PROCESSING", "正在执行公式计算...");

            using var scope = _serviceProvider.CreateScope();
            var calculator = scope.ServiceProvider.GetRequiredService<IntermediateDataFormulaBatchCalculator>();
            calculator.CreatorUserId = state.UserId;

            Dictionary<string, UnitPrecisionInfo>? unitPrecisions = null;
            if (!string.IsNullOrWhiteSpace(state.UnitPrecisionsJson))
            {
                unitPrecisions = JsonSerializer.Deserialize<Dictionary<string, UnitPrecisionInfo>>(state.UnitPrecisionsJson);
            }

            var result = await calculator.CalculateByBatchAsync(batchId, unitPrecisions);

            _logger.LogInformation(
                "公式计算完成: BatchId={BatchId}, 公式成功={Success}, 公式失败={Failed}",
                batchId, result.SuccessCount, result.FailedCount);

            // ── 阶段 B: 自动判定（对计算成功的数据） ──
            var judgeResult = await RunJudgmentPhase(scope, calculator, batchId, state);

            // ── 汇总最终状态 ──
            var totalFailed = Volatile.Read(ref state.Failed) + result.FailedCount + judgeResult.FailedCount;
            var totalSuccess = Volatile.Read(ref state.Success) + result.SuccessCount;
            var finalStatus = totalFailed > 0 && totalSuccess == 0 ? "FAILED" : "COMPLETED";

            var msg = $"完成: 基础计算成功 {state.Success} 条, 公式成功 {result.SuccessCount} 条";
            if (judgeResult.TotalCount > 0)
                msg += $", 判定成功 {judgeResult.SuccessCount} 条";
            if (totalFailed > 0)
                msg += $", 失败 {totalFailed} 条";

            PublishProgressFromState(state, finalStatus, msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "公式计算/判定异常: BatchId={BatchId}", batchId);
            PublishProgressFromState(state, "FAILED", $"计算异常: {ex.Message}");
        }
        finally
        {
            // 清理内存中的批次状态
            _progressTracker.Remove(batchId);
        }
    }

    /// <summary>
    /// 执行判定阶段：查询 CalcStatus==SUCCESS 的数据并调用判定。
    /// 用于计算完成后自动判定（触发点 1）。
    /// </summary>
    private async Task<FormulaCalculationResult> RunJudgmentPhase(
        IServiceScope scope,
        IntermediateDataFormulaBatchCalculator calculator,
        string batchId,
        BatchState state)
    {
        var emptyResult = new FormulaCalculationResult();

        try
        {
            PublishProgressFromState(state, "PROCESSING", "正在执行判定...");

            var intermediateRepo = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<IntermediateDataEntity>>();

            // 查询本批次中计算成功的数据 ID
            var successIds = await intermediateRepo
                .AsQueryable()
                .Where(e => e.BatchId == batchId
                    && e.CalcStatus == IntermediateDataCalcStatus.SUCCESS
                    && e.DeleteMark == null)
                .Select(e => e.Id)
                .ToListAsync();

            if (successIds.Count == 0)
            {
                _logger.LogInformation("BatchId={BatchId}: 无计算成功的数据，跳过判定", batchId);
                return emptyResult;
            }

            _logger.LogInformation("BatchId={BatchId}: 开始判定 {Count} 条数据", batchId, successIds.Count);

            // 分块判定 + 进度推送
            var judgeChunkSize = 50;
            var totalJudge = successIds.Count;
            var judged = 0;
            var judgeSuccess = 0;
            var judgeFailed = 0;

            var chunks = successIds.Chunk(judgeChunkSize);
            foreach (var chunk in chunks)
            {
                var chunkResult = await calculator.JudgeByIdsAsync(chunk.ToList());
                judgeSuccess += chunkResult.SuccessCount;
                judgeFailed += chunkResult.FailedCount;
                judged += chunk.Length;

                PublishProgressFromState(state, "PROCESSING",
                    $"判定中: {judged}/{totalJudge} (成功 {judgeSuccess}, 失败 {judgeFailed})");
            }

            _logger.LogInformation(
                "判定完成: BatchId={BatchId}, 成功={Success}, 失败={Failed}",
                batchId, judgeSuccess, judgeFailed);

            return new FormulaCalculationResult
            {
                TotalCount = totalJudge,
                SuccessCount = judgeSuccess,
                FailedCount = judgeFailed,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "判定阶段异常: BatchId={BatchId}", batchId);
            return emptyResult;
        }
    }

    /// <summary>
    /// 从 BatchState 生成并发布进度消息。
    /// </summary>
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

    // ═════════════════════════════════════════════════════════════════════════
    //  旧 batch 模式（向后兼容）
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 每批推送进度的条数。
    /// </summary>
    private const int ProgressBatchSize = 10;

    /// <summary>
    /// 处理导入任务（旧 batch 模式，向后兼容）：
    /// 骨架中间数据已由 API 在事务中写入（CalcStatus=PENDING），
    /// Worker 只需查出 → 逐条计算填充 → 批量更新 → 公式计算。
    /// </summary>
    private async Task ProcessImportTask(IServiceScope scope, CalcTaskMessage task)
    {
        _logger.LogInformation(
            "开始处理计算任务(batch模式): BatchId={BatchId}, TaskType={TaskType}, TotalCount={Count}",
            task.BatchId, task.TaskType, task.TotalCount);

        PublishProgress(task, 0, 0, 0, "PROCESSING", "计算任务开始...");

        var intermediateRepo = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<IntermediateDataEntity>>();
        var generator = scope.ServiceProvider.GetRequiredService<IntermediateDataGenerator>();
        var sqlSugarClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

        // ── 阶段 1/3：查询骨架中间数据 ──
        PublishProgress(task, 0, 0, 0, "PROCESSING", "正在查询中间数据...");

        var entities = await intermediateRepo
            .AsQueryable()
            .Where(e => e.BatchId == task.BatchId)
            .ToListAsync();

        if (entities.Count == 0)
        {
            _logger.LogWarning("BatchId={BatchId}: 未找到中间数据", task.BatchId);
            PublishProgress(task, 0, 0, 0, "COMPLETED", "无数据需要计算");
            return;
        }

        var totalCount = entities.Count;
        task.TotalCount = totalCount;

        _logger.LogInformation("查询到 {Count} 条待计算中间数据", totalCount);

        // ── 阶段 2/3：逐条计算 + 分批更新 DB ──
        int calcSuccess = 0;
        int calcFailed = 0;
        int processed = 0;

        var chunks = entities.Chunk(ProgressBatchSize);

        foreach (var chunk in chunks)
        {
            foreach (var entity in chunk)
            {
                try
                {
                    await generator.CalculateFromEntityAsync(entity);
                    entity.CalcStatus = IntermediateDataCalcStatus.SUCCESS;
                    entity.CalcStatusTime = DateTime.Now;
                    calcSuccess++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "中间数据计算失败: Id={Id}", entity.Id);
                    entity.CalcStatus = IntermediateDataCalcStatus.FAILED;
                    entity.CalcErrorMessage = ex.Message;
                    entity.CalcStatusTime = DateTime.Now;
                    calcFailed++;
                }
            }

            await sqlSugarClient.Updateable(chunk.ToList()).ExecuteCommandAsync();

            processed += chunk.Length;
            PublishProgress(task, processed, calcSuccess, calcFailed, "PROCESSING",
                $"基础计算: {processed}/{totalCount} (成功 {calcSuccess}, 失败 {calcFailed})");
        }

        _logger.LogInformation("基础计算全部完成: 成功={Success}, 失败={Failed}，开始公式计算...", calcSuccess, calcFailed);

        // ── 阶段 3/3：公式计算 ──
        PublishProgress(task, processed, calcSuccess, calcFailed, "PROCESSING", "正在执行公式计算...");

        var calculator = scope.ServiceProvider.GetRequiredService<IntermediateDataFormulaBatchCalculator>();
        calculator.CreatorUserId = task.UserId;

        Dictionary<string, UnitPrecisionInfo>? unitPrecisions = null;
        if (!string.IsNullOrWhiteSpace(task.UnitPrecisionsJson))
        {
            unitPrecisions = JsonSerializer.Deserialize<Dictionary<string, UnitPrecisionInfo>>(task.UnitPrecisionsJson);
        }

        var result = await calculator.CalculateByBatchAsync(task.BatchId, unitPrecisions);

        _logger.LogInformation(
            "公式计算完成: BatchId={BatchId}, 公式成功={Success}, 公式失败={Failed}",
            task.BatchId, result.SuccessCount, result.FailedCount);

        // ── 阶段 4/4：自动判定 ──
        PublishProgress(task, totalCount, result.SuccessCount, calcFailed + result.FailedCount, "PROCESSING", "正在执行判定...");

        var intermediateRepo2 = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<IntermediateDataEntity>>();
        var successIds = await intermediateRepo2
            .AsQueryable()
            .Where(e => e.BatchId == task.BatchId
                && e.CalcStatus == IntermediateDataCalcStatus.SUCCESS
                && e.DeleteMark == null)
            .Select(e => e.Id)
            .ToListAsync();

        int judgeSuccess = 0, judgeFailed = 0;
        if (successIds.Count > 0)
        {
            _logger.LogInformation("BatchId={BatchId}: 开始判定 {Count} 条", task.BatchId, successIds.Count);
            const int judgeChunkSize = 50;
            int judged = 0;
            foreach (var chunk in successIds.Chunk(judgeChunkSize))
            {
                var jr = await calculator.JudgeByIdsAsync(chunk.ToList());
                judgeSuccess += jr.SuccessCount;
                judgeFailed += jr.FailedCount;
                judged += chunk.Length;
                PublishProgress(task, totalCount, result.SuccessCount, calcFailed + result.FailedCount + judgeFailed, "PROCESSING",
                    $"判定中: {judged}/{successIds.Count} (成功 {judgeSuccess}, 失败 {judgeFailed})");
            }
        }

        var totalFailed = calcFailed + result.FailedCount + judgeFailed;
        var finalStatus = totalFailed > 0 && (calcSuccess + result.SuccessCount) == 0 ? "FAILED" : "COMPLETED";

        var msg = $"完成: 基础计算成功 {calcSuccess} 条, 公式成功 {result.SuccessCount} 条";
        if (successIds.Count > 0)
            msg += $", 判定成功 {judgeSuccess} 条";
        if (totalFailed > 0)
            msg += $", 失败 {totalFailed} 条";

        PublishProgress(task, totalCount, result.SuccessCount, totalFailed, finalStatus, msg);
    }

    /// <summary>
    /// 处理已有中间数据的重算任务（JUDGE 任务已迁移到 JudgeTaskConsumer）。
    /// </summary>
    private async Task ProcessExistingData(IServiceScope scope, CalcTaskMessage task)
    {
        _logger.LogInformation(
            "开始处理重算任务: BatchId={BatchId}, TaskType={TaskType}, Count={Count}",
            task.BatchId, task.TaskType, task.IntermediateDataIds?.Count ?? 0);

        PublishProgress(task, 0, 0, 0, "PROCESSING", "计算任务开始...");

        var calculator = scope.ServiceProvider.GetRequiredService<IntermediateDataFormulaBatchCalculator>();
        calculator.CreatorUserId = task.UserId;

        Dictionary<string, UnitPrecisionInfo>? unitPrecisions = null;
        if (!string.IsNullOrWhiteSpace(task.UnitPrecisionsJson))
        {
            unitPrecisions = JsonSerializer.Deserialize<Dictionary<string, UnitPrecisionInfo>>(task.UnitPrecisionsJson);
        }

        var result = await calculator.CalculateByIdsAsync(task.IntermediateDataIds!, unitPrecisions);

        _logger.LogInformation(
            "重算完成: BatchId={BatchId}, 成功={Success}, 失败={Failed}",
            task.BatchId, result.SuccessCount, result.FailedCount);

        var status = result.FailedCount > 0 && result.SuccessCount == 0 ? "FAILED" : "COMPLETED";
        PublishProgress(task, result.TotalCount, result.SuccessCount, result.FailedCount, status, result.Message ?? "完成");
    }

    /// <summary>
    /// 发布进度消息到队列（用于 batch 模式）。
    /// </summary>
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("计算 Worker 正在停止...");

        _channel?.Close();
        _connection?.Close();

        await base.StopAsync(cancellationToken);
    }
}
