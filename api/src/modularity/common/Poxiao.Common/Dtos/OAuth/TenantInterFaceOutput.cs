namespace Poxiao.Infrastructure.Dtos.OAuth;

/// <summary>
/// 多租户网络连接输出.
/// </summary>
[SuppressSniffer]
public class TenantInterFaceOutput
{
    /// <summary>
    /// DotNet.
    /// </summary>
    public string dotnet { get; set; }

    public List<TenantLinkModel>? linkList { get; set; }
}