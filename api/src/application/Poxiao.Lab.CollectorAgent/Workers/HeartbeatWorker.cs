using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poxiao.Lab.CollectorAgent.Options;
using Poxiao.Lab.CollectorAgent.Upload;

namespace Poxiao.Lab.CollectorAgent.Workers;

/// <summary>
/// 心跳 Worker：定时向中心服务器上报存活状态，失败仅记 WARN（由 ServerClient 内部处理），不影响采集/上报主流程。
/// </summary>
public class HeartbeatWorker : BackgroundService
{
    private readonly CollectorOptions _options;
    private readonly ServerClient _serverClient;
    private readonly ILogger<HeartbeatWorker> _logger;

    public HeartbeatWorker(IOptions<CollectorOptions> options, ServerClient serverClient, ILogger<HeartbeatWorker> logger)
    {
        _options = options.Value;
        _serverClient = serverClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(10, _options.HeartbeatIntervalSeconds));
        _logger.LogInformation("HeartbeatWorker 启动，间隔 {Interval}s。", interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ok = await _serverClient.SendHeartbeatAsync(stoppingToken);
                if (ok)
                {
                    _logger.LogDebug("心跳上报成功。");
                }

                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("HeartbeatWorker 已停止。");
    }
}
