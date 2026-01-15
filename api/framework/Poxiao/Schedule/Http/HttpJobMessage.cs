namespace Poxiao.Schedule;

/// <summary>
/// HTTP 作业消息
/// </summary>
[SuppressSniffer]
public sealed class HttpJobMessage
{
    /// <summary>
    /// 请求地址
    /// </summary>
    public string RequestUri { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

    /// <summary>
    /// 请求报文体
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// 请求客户端名称
    /// </summary>
    public string ClientName { get; set; } = nameof(HttpJob);

    /// <summary>
    /// 确保请求成功，否则抛异常
    /// </summary>
    public bool EnsureSuccessStatusCode { get; set; } = true;
}