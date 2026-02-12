using Microsoft.Extensions.Logging;
using Poxiao.DependencyInjection;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Poxiao.Lab.Service;

/// <summary>
/// 计算任务发布器 — 将计算任务发送到 lab.calc.task 队列。
/// 在 API 进程中使用，替代原来的 EventPublisher 方式。
/// </summary>
public class CalcTaskPublisher : ISingleton, IDisposable
{
    private readonly ILogger<CalcTaskPublisher> _logger;

    private IConnection? _connection;
    private IModel? _channel;
    private string _calcQueueName = "lab.calc.task";
    private string _judgeQueueName = "lab.judge.task";
    private bool _initialized;
    private bool _disposed;
    private readonly object _lock = new();

    public CalcTaskPublisher(ILogger<CalcTaskPublisher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 初始化 RabbitMQ 连接（在 Startup 中调用）。
    /// </summary>
    public void Initialize(
        string hostName,
        int port,
        string userName,
        string password,
        string? taskQueueName = null,
        string? judgeQueueName = null)
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;

            if (!string.IsNullOrWhiteSpace(taskQueueName))
            {
                _calcQueueName = taskQueueName;
            }

            if (!string.IsNullOrWhiteSpace(judgeQueueName))
            {
                _judgeQueueName = judgeQueueName;
            }

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = hostName,
                    Port = port,
                    UserName = userName,
                    Password = password,
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // 声明计算队列
                _channel.QueueDeclare(
                    queue: _calcQueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // 声明判定队列
                _channel.QueueDeclare(
                    queue: _judgeQueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _initialized = true;
                _logger.LogInformation(
                    "CalcTaskPublisher: 已连接 RabbitMQ，计算队列: {CalcQueue}, 判定队列: {JudgeQueue}",
                    _calcQueueName, _judgeQueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CalcTaskPublisher: 连接 RabbitMQ 失败");
            }
        }
    }

    /// <summary>
    /// 发布批次计算任务（旧模式，保留兼容）。
    /// </summary>
    public void PublishCalcTask(
        string batchId,
        string tenantId,
        string userId,
        string sessionId,
        int totalCount,
        Dictionary<string, UnitPrecisionInfo>? unitPrecisions = null)
    {
        var message = new CalcTaskMessage
        {
            BatchId = batchId,
            TenantId = tenantId,
            UserId = userId,
            SessionId = sessionId,
            TotalCount = totalCount,
            TaskType = "CALC",
            UnitPrecisionsJson = unitPrecisions != null
                ? JsonSerializer.Serialize(unitPrecisions)
                : string.Empty,
        };

        Publish(message);
    }

    /// <summary>
    /// 发布 per-item 计算任务：每条中间数据一条 MQ 消息，Worker 端并发消费。
    /// </summary>
    public void PublishCalcItems(
        string batchId,
        List<string> intermediateDataIds,
        string tenantId,
        string userId,
        Dictionary<string, UnitPrecisionInfo>? unitPrecisions = null)
    {
        if (!_initialized || _channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("CalcTaskPublisher: RabbitMQ 未连接，无法发布任务");
            return;
        }

        var totalCount = intermediateDataIds.Count;
        var unitPrecisionsJson = unitPrecisions != null
            ? JsonSerializer.Serialize(unitPrecisions)
            : string.Empty;

        try
        {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            foreach (var id in intermediateDataIds)
            {
                var message = new CalcTaskMessage
                {
                    BatchId = batchId,
                    TenantId = tenantId,
                    UserId = userId,
                    TotalCount = totalCount,
                    TaskType = "CALC",
                    IntermediateDataId = id,
                    UnitPrecisionsJson = unitPrecisionsJson,
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: _calcQueueName,
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogInformation(
                "CalcTaskPublisher: 已发布 {Count} 条 per-item 计算任务到 {Queue}, BatchId={BatchId}",
                totalCount, _calcQueueName, batchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalcTaskPublisher: 批量发布任务失败, BatchId={BatchId}", batchId);
        }
    }

    /// <summary>
    /// 发布按 ID 列表的计算任务（手动重算）。
    /// </summary>
    public void PublishCalcByIds(
        List<string> ids,
        string tenantId,
        string userId)
    {
        var message = new CalcTaskMessage
        {
            TenantId = tenantId,
            UserId = userId,
            TotalCount = ids.Count,
            TaskType = "CALC",
            IntermediateDataIds = ids,
        };

        Publish(message);
    }

    /// <summary>
    /// 发布 per-item 磁性更新+判定任务：每条中间数据一条 MQ 消息。
    /// Worker 端并发消费，执行磁性字段局部更新 + 判定。
    /// </summary>
    public void PublishMagneticJudgeItems(
        string batchId,
        List<MagneticDataPayload> items,
        string tenantId,
        string userId)
    {
        if (!_initialized || _channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("CalcTaskPublisher: RabbitMQ 未连接，无法发布任务");
            return;
        }

        var totalCount = items.Count;

        try
        {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            foreach (var payload in items)
            {
                var message = new CalcTaskMessage
                {
                    BatchId = batchId,
                    TenantId = tenantId,
                    UserId = userId,
                    TotalCount = totalCount,
                    TaskType = "MAGNETIC_JUDGE",
                    // 使用炉号作为 per-item 标识（Worker 端通过炉号查找中间数据 ID）
                    IntermediateDataId = payload.FurnaceNo,
                    MagneticDataJson = JsonSerializer.Serialize(payload),
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: _judgeQueueName,
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogInformation(
                "CalcTaskPublisher: 已发布 {Count} 条磁性更新+判定任务到 {Queue}, BatchId={BatchId}",
                totalCount, _judgeQueueName, batchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalcTaskPublisher: 发布磁性更新+判定任务失败, BatchId={BatchId}", batchId);
        }
    }

    /// <summary>
    /// 发布判定任务：每条中间数据一条 MQ 消息，Worker 端逐条消费并上报进度。
    /// </summary>
    public void PublishJudgeTask(
        List<string> ids,
        string tenantId,
        string userId,
        string? batchId = null)
    {
        if (ids == null || ids.Count == 0)
        {
            return;
        }

        var bid = batchId ?? Guid.NewGuid().ToString();
        var totalCount = ids.Count;

        if (!_initialized || _channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("CalcTaskPublisher: RabbitMQ 未连接，无法发布判定任务");
            return;
        }

        try
        {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            foreach (var id in ids)
            {
                var message = new CalcTaskMessage
                {
                    BatchId = bid,
                    TenantId = tenantId,
                    UserId = userId,
                    TotalCount = totalCount,
                    TaskType = "JUDGE",
                    IntermediateDataId = id,
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);
                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: _judgeQueueName,
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogInformation(
                "CalcTaskPublisher: 已发布 {Count} 条判定任务到 {Queue}, BatchId={BatchId}",
                totalCount, _judgeQueueName, bid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalcTaskPublisher: 发布判定任务失败, BatchId={BatchId}", bid);
        }
    }

    private void Publish(CalcTaskMessage message)
    {
        if (!_initialized || _channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("CalcTaskPublisher: RabbitMQ 未连接，无法发布任务");
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            // 根据 TaskType 路由到不同队列：JUDGE/MAGNETIC_JUDGE → 判定队列，其他 → 计算队列
            var routingKey = message.TaskType is "JUDGE" or "MAGNETIC_JUDGE"
                ? _judgeQueueName
                : _calcQueueName;

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "CalcTaskPublisher: 已发布 {TaskType} 任务到 {Queue}, BatchId={BatchId}, Count={Count}",
                message.TaskType, routingKey, message.BatchId, message.TotalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalcTaskPublisher: 发布任务失败");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
