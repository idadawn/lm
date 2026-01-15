using Poxiao.DependencyInjection;

namespace SqlSugar;

/// <summary>
/// 全局租户缓存模型.
/// </summary>
[SuppressSniffer]
public class GlobalTenantCacheModel
{
    /// <summary>
    /// 租户ID.
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// 登录方式.
    /// </summary>
    public int SingleLogin { get; set; }

    /// <summary>
    /// 连接配置.
    /// </summary>
    public ConnectionConfigOptions connectionConfig { get; set; }
}