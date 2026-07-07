using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Poxiao.Lab.CollectorAgent.Options;
using Poxiao.Lab.CollectorAgent.Spool;

namespace Poxiao.Lab.CollectorAgent.Upload;

/// <summary>
/// 与中心服务器交互的 HTTP 客户端：批量数据上报 + 心跳。
/// </summary>
public class ServerClient
{
    private readonly HttpClient _httpClient;
    private readonly CollectorOptions _options;
    private readonly ILogger<ServerClient> _logger;

    public ServerClient(HttpClient httpClient, IOptions<CollectorOptions> options, ILogger<ServerClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.ServerBaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// 上报一个批次。
    /// </summary>
    public async Task<UploadResult> UploadBatchAsync(SpoolBatch batch, CancellationToken cancellationToken)
    {
        var payload = new
        {
            batch.BatchId,
            batch.SourceName,
            batch.DeviceCode,
            batch.CreatedAt,
            CollectorId = _options.CollectorId,
            batch.Records,
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, CollectorHeaders.BatchUploadPath.TrimStart('/'))
            {
                Content = JsonContent.Create(payload),
            };
            AddAuthHeaders(request);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return UploadResult.Success();
            }

            return ClassifyFailure(response.StatusCode, await SafeReadBodyAsync(response, cancellationToken));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient 超时
            return UploadResult.Retryable("请求超时");
        }
        catch (HttpRequestException ex)
        {
            return UploadResult.Retryable($"网络异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送心跳，失败仅由调用方记 WARN，不抛异常向上传播关键错误信息。
    /// </summary>
    public async Task<bool> SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                CollectorId = _options.CollectorId,
                Timestamp = DateTimeOffset.Now,
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, CollectorHeaders.HeartbeatPath.TrimStart('/'))
            {
                Content = JsonContent.Create(payload),
            };
            AddAuthHeaders(request);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "心跳上报失败：{Message}", ex.Message);
            return false;
        }
    }

    private void AddAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation(CollectorHeaders.AppIdHeader, _options.AppId);
        request.Headers.TryAddWithoutValidation(CollectorHeaders.AppSecretHeader, _options.AppSecret);
    }

    private static UploadResult ClassifyFailure(HttpStatusCode statusCode, string body)
    {
        var code = (int)statusCode;
        if (code == 401 || code == 403)
        {
            return UploadResult.NonRetryable($"鉴权失败 ({(int)statusCode} {statusCode}): {body}");
        }

        if (code >= 400 && code < 500)
        {
            return UploadResult.NonRetryable($"请求被拒绝 ({(int)statusCode} {statusCode}): {body}");
        }

        // 5xx 及其他视为可重试（服务端临时故障）
        return UploadResult.Retryable($"服务端错误 ({(int)statusCode} {statusCode}): {body}");
    }

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return string.Empty;
        }
    }
}

/// <summary>
/// 批量上报结果：区分成功 / 可重试失败（网络、5xx）/ 不可重试失败（4xx 鉴权类）。
/// </summary>
public class UploadResult
{
    public bool IsSuccess { get; private init; }

    public bool IsRetryable { get; private init; }

    public string? Message { get; private init; }

    public static UploadResult Success() => new() { IsSuccess = true };

    public static UploadResult Retryable(string message) => new() { IsSuccess = false, IsRetryable = true, Message = message };

    public static UploadResult NonRetryable(string message) => new() { IsSuccess = false, IsRetryable = false, Message = message };
}
