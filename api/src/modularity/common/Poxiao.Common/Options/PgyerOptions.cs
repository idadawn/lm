using Poxiao.ConfigurableOptions;

namespace Poxiao.Infrastructure.Options;

/// <summary>
/// 蒲公英(Pgyer)服务端配置.
/// 仅服务端持有，用于后端代理 app/check；切勿下发到客户端.
/// 生产建议通过环境变量注入：Pgyer__ApiKey / Pgyer__AppKey.
/// </summary>
public sealed class PgyerOptions : IConfigurableOptions
{
    /// <summary>
    /// 蒲公英账号级 API Key（_api_key）.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// 应用 Key（appKey）.
    /// </summary>
    public string AppKey { get; set; }
}
