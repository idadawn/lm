using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Poxiao.Infrastructure.Core.Handlers;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Poxiao.Lab.Service;

/// <summary>
/// 计算进度消费者 — 在 API 进程中运行的 BackgroundService。
/// 监听 lab.calc.progress 队列，将进度通过 WebSocket 推送给前端。
/// </summary>
public class CalcProgressConsumer : BackgroundService
{
    private readonly ILogger<CalcProgressConsumer> _logger;
    private readonly IMHandler _imHandler;

    private string _hostName = "localhost";
    private int _port = 5672;
    private string _userName = "guest";
    private string _password = "guest";
    private string _queueName = "lab.calc.progress";
    private bool _configured;

    public CalcProgressConsumer(
        ILogger<CalcProgressConsumer> logger,
        IMHandler imHandler)
    {
        _logger = logger;
        _imHandler = imHandler;
    }

    /// <summary>
    /// 配置 RabbitMQ 连接参数（在 Startup 中调用）。
    /// </summary>
    public void Configure(string hostName, int port, string userName, string password, string? queueName = null)
    {
        _hostName = hostName;
        _port = port;
        _userName = userName;
        _password = password;
        if (!string.IsNullOrWhiteSpace(queueName))
        {
            _queueName = queueName;
        }

        _configured = true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 等待一小段时间让 Host 完全启动
        await Task.Delay(2000, stoppingToken);

        if (!_configured)
        {
            _logger.LogWarning("CalcProgressConsumer: 未配置 RabbitMQ 连接参数，跳过启动");
            return;
        }

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
                _logger.LogError(ex, "CalcProgressConsumer: RabbitMQ 消费异常，5 秒后重连...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ConsumeLoop(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName,
            Password = _password,
            DispatchConsumersAsync = true,
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 5, global: false);

        _logger.LogInformation("CalcProgressConsumer: 已连接 RabbitMQ，监听进度队列: {Queue}", _queueName);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var progress = JsonSerializer.Deserialize<CalcProgressMessage>(body);

                if (progress != null)
                {
                    await PushToWebSocket(progress);
                }

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CalcProgressConsumer: 处理进度消息失败");
                channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        channel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        // 保持运行直到取消
        var tcs = new TaskCompletionSource<bool>();
        stoppingToken.Register(() => tcs.TrySetResult(true));
        await tcs.Task;
    }

    private static readonly JsonSerializerOptions _camelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// 将进度消息通过 WebSocket 推送给对应用户。
    /// 注意：前端 TypeScript 接口使用 camelCase，必须使用 CamelCase 序列化策略。
    /// </summary>
    private async Task PushToWebSocket(CalcProgressMessage progress)
    {
        var userKey = $"{progress.TenantId}-{progress.UserId}";

        var message = JsonSerializer.Serialize(new
        {
            method = "calcProgress",
            data = new
            {
                progress.BatchId,
                progress.TaskType,
                progress.Total,
                progress.Completed,
                progress.SuccessCount,
                progress.FailedCount,
                progress.Status,
                progress.Message,
                progress.Timestamp,
            },
        }, _camelCaseOptions);

        try
        {
            await _imHandler.SendMessageToUserAsync(userKey, message);

            _logger.LogDebug(
                "CalcProgressConsumer: 已推送进度给用户 {UserKey}, Status={Status}, {Completed}/{Total}",
                userKey, progress.Status, progress.Completed, progress.Total);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CalcProgressConsumer: WebSocket 推送失败 (用户可能离线): {UserKey}", userKey);
        }

        // 计算完成时，同时推送全局通知消息
        if (progress.Status is "COMPLETED" or "FAILED")
        {
            await PushNotification(progress, userKey);
        }
    }

    /// <summary>
    /// 推送计算完成的全局通知（右上角 bell icon）。
    /// </summary>
    private async Task PushNotification(CalcProgressMessage progress, string userKey)
    {
        var title = progress.Status == "COMPLETED" ? "计算完成" : "计算失败";
        var content = progress.Status == "COMPLETED"
            ? $"批次数据计算完成：共 {progress.Total} 条，成功 {progress.SuccessCount} 条" +
              (progress.FailedCount > 0 ? $"，失败 {progress.FailedCount} 条" : string.Empty)
            : $"批次数据计算失败：{progress.Message}";

        var notification = JsonSerializer.Serialize(new
        {
            method = "calcNotification",
            data = new
            {
                title,
                content,
                type = progress.Status == "COMPLETED" ? "success" : "error",
                batchId = progress.BatchId,
                taskType = progress.TaskType,
                timestamp = progress.Timestamp,
            },
        }, _camelCaseOptions);

        try
        {
            await _imHandler.SendMessageToUserAsync(userKey, notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CalcProgressConsumer: 推送通知失败: {UserKey}", userKey);
        }
    }
}
