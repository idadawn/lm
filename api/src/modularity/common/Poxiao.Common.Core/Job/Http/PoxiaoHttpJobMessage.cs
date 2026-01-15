using Poxiao.DependencyInjection;

namespace Poxiao.Infrastructure.Core.Job;

/// <summary>
/// Poxiao-HTTP 作业消息.
/// </summary>
[SuppressSniffer]
public class PoxiaoHttpJobMessage
{
    /// <summary>
    /// 请求地址.
    /// </summary>
    public string RequestUri { get; set; }

    /// <summary>
    /// 请求方法.
    /// </summary>
    public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

    /// <summary>
    /// 请求报文体.
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// 任务ID.
    /// </summary>
    public string TaskId { get; set; }

    /// <summary>
    /// 用户ID.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// 租户ID.
    /// </summary>
    public string TenantId { get; set; }
}