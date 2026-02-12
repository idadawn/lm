using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Poxiao.Lab.CalcWorker.Services;

/// <summary>
/// 计算进度发布器 — 将进度消息发布到 lab.calc.progress 队列。
/// </summary>
public sealed class CalcProgressPublisher : IDisposable
{
    private readonly ILogger<CalcProgressPublisher> _logger;
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    public CalcProgressPublisher(IOptions<RabbitMqOptions> options, ILogger<CalcProgressPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 确保 RabbitMQ 连接已建立。
    /// </summary>
    private void EnsureConnection()
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _options.ProgressQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("CalcProgressPublisher: 已连接 RabbitMQ，进度队列: {Queue}", _options.ProgressQueueName);
    }

    /// <summary>
    /// 发布进度消息。
    /// </summary>
    public void Publish<T>(T message)
    {
        try
        {
            EnsureConnection();

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _options.ProgressQueueName,
                basicProperties: properties,
                body: body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalcProgressPublisher: 发布进度消息失败");
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
