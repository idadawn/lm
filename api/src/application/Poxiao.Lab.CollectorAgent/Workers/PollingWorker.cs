using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poxiao.Lab.CollectorAgent.Options;
using Poxiao.Lab.CollectorAgent.Sources;
using Poxiao.Lab.CollectorAgent.Spool;
using Poxiao.Lab.CollectorAgent.State;

namespace Poxiao.Lab.CollectorAgent.Workers;

/// <summary>
/// 轮询 Worker：为每个启用的数据源独立开一个轮询循环（Task.WhenAll 并行），
/// 采集到新记录后先落盘 Spool，再推进 lastPosition —— 保证至少一次投递语义
/// （若进程在落盘后、更新位点前崩溃，重启后会重复采集，但不会丢数据）。
/// 单个数据源异常不影响其他源，按源独立退避重试（5s -> 60s 封顶）。
/// </summary>
public class PollingWorker : BackgroundService
{
    private static readonly TimeSpan MinBackoff = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(60);

    private readonly CollectorOptions _options;
    private readonly SourceStateStore _stateStore;
    private readonly SpoolQueue _spoolQueue;
    private readonly ILogger<PollingWorker> _logger;

    public PollingWorker(
        IOptions<CollectorOptions> options,
        SourceStateStore stateStore,
        SpoolQueue spoolQueue,
        ILogger<PollingWorker> logger)
    {
        _options = options.Value;
        _stateStore = stateStore;
        _spoolQueue = spoolQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabledSources = _options.Sources.Where(s => s.Enabled).ToList();
        if (enabledSources.Count == 0)
        {
            _logger.LogWarning("未配置任何启用的数据源，PollingWorker 不执行任何采集。");
            return;
        }

        var tasks = enabledSources.Select(source => RunSourceLoopAsync(source, stoppingToken));
        await Task.WhenAll(tasks);
    }

    private async Task RunSourceLoopAsync(SourceOptions source, CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "数据源 '{Source}'（{Type}/{DeviceCode}）轮询循环启动，间隔 {Interval}s。",
            source.Name,
            source.Type,
            source.DeviceCode,
            source.PollIntervalSeconds);

        IDeviceDataSource? dataSource = null;
        var backoff = MinBackoff;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                dataSource ??= CreateDataSource(source);

                var lastPosition = await _stateStore.GetLastPositionAsync(source.Name, stoppingToken);
                var records = await dataSource.FetchNewAsync(lastPosition, source.BatchSize, stoppingToken);

                if (records.Count > 0)
                {
                    await _spoolQueue.EnqueueAsync(source.Name, source.DeviceCode, records, stoppingToken);

                    var newPosition = records[^1].Position;
                    if (!string.IsNullOrEmpty(newPosition))
                    {
                        await _stateStore.SetLastPositionAsync(source.Name, newPosition, stoppingToken);
                    }

                    _logger.LogInformation(
                        "数据源 '{Source}' 采集到 {Count} 条新记录，位点推进至 {Position}。",
                        source.Name,
                        records.Count,
                        newPosition);
                }

                backoff = MinBackoff;
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, source.PollIntervalSeconds)), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据源 '{Source}' 采集失败，{Backoff}s 后重试。", source.Name, backoff.TotalSeconds);
                try
                {
                    await Task.Delay(backoff, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                backoff = TimeSpan.FromSeconds(Math.Min(backoff.TotalSeconds * 2, MaxBackoff.TotalSeconds));
            }
        }

        _logger.LogInformation("数据源 '{Source}' 轮询循环已停止。", source.Name);
    }

    private static IDeviceDataSource CreateDataSource(SourceOptions source)
    {
        return source.Type.Trim().ToLowerInvariant() switch
        {
            "mock" => new MockDataSource(source),
            "access" => new AccessDataSource(source),
            _ => throw new NotSupportedException($"数据源 '{source.Name}' 的类型 '{source.Type}' 不受支持（支持 mock / access）。"),
        };
    }
}
