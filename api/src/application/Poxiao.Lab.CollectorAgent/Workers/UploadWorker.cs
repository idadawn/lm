using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Poxiao.Lab.CollectorAgent.Spool;
using Poxiao.Lab.CollectorAgent.Upload;

namespace Poxiao.Lab.CollectorAgent.Workers;

/// <summary>
/// 上报 Worker：循环 drain 本地 Spool 队列（最旧优先），成功即删除批次文件；
/// 可重试失败（网络异常 / 5xx）按指数退避重试（5s -> 300s 封顶）；
/// 不可重试失败（4xx 鉴权错误）记 ERROR 日志并保留文件等待人工介入，固定退避 300s 后再次尝试。
/// </summary>
public class UploadWorker : BackgroundService
{
    private static readonly TimeSpan MinBackoff = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(300);
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(3);

    private readonly SpoolQueue _spoolQueue;
    private readonly ServerClient _serverClient;
    private readonly ILogger<UploadWorker> _logger;

    public UploadWorker(SpoolQueue spoolQueue, ServerClient serverClient, ILogger<UploadWorker> logger)
    {
        _spoolQueue = spoolQueue;
        _serverClient = serverClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UploadWorker 启动，开始 drain 本地暂存队列。");

        var backoff = MinBackoff;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var filePath = _spoolQueue.PeekOldest();
                if (filePath is null)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                    continue;
                }

                var batch = await _spoolQueue.ReadAsync(filePath, stoppingToken);
                if (batch is null)
                {
                    _logger.LogWarning("暂存批次文件无法解析，移除：{File}", filePath);
                    _spoolQueue.Remove(filePath);
                    continue;
                }

                var result = await _serverClient.UploadBatchAsync(batch, stoppingToken);
                if (result.IsSuccess)
                {
                    _spoolQueue.Remove(filePath);
                    _logger.LogInformation(
                        "批次 {BatchId}（源 {Source}，{Count} 条）上报成功。",
                        batch.BatchId,
                        batch.SourceName,
                        batch.Records.Count);
                    backoff = MinBackoff;
                    continue;
                }

                if (result.IsRetryable)
                {
                    _logger.LogWarning(
                        "批次 {BatchId}（源 {Source}）上报失败（可重试）：{Message}，{Backoff}s 后重试。",
                        batch.BatchId,
                        batch.SourceName,
                        result.Message,
                        backoff.TotalSeconds);
                    await Task.Delay(backoff, stoppingToken);
                    backoff = TimeSpan.FromSeconds(Math.Min(backoff.TotalSeconds * 2, MaxBackoff.TotalSeconds));
                }
                else
                {
                    _logger.LogError(
                        "批次 {BatchId}（源 {Source}）上报被服务端拒绝（不可重试）：{Message}，文件保留待人工介入，300s 后再次尝试。",
                        batch.BatchId,
                        batch.SourceName,
                        result.Message);
                    await Task.Delay(MaxBackoff, stoppingToken);
                    backoff = MinBackoff;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadWorker 处理批次时发生未预期异常，{Backoff}s 后重试。", backoff.TotalSeconds);
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

        _logger.LogInformation("UploadWorker 已停止。");
    }
}
