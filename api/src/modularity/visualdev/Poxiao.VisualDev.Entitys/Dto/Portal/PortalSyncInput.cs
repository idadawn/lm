using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Dto.System.Portal;

/// <summary>
/// 门户同步输入.
/// </summary>
[SuppressSniffer]
public class PortalSyncInput
{
    /// <summary>
    /// 同步至pc端（0：不同步，1：同步）.
    /// </summary>
    public int? pc { get; set; }

    /// <summary>
    /// 同步至app端（0：不同步，1：同步）.
    /// </summary>
    public int? app { get; set; }

    /// <summary>
    /// app上级id.
    /// </summary>
    public List<string> appModuleParentId { get; set; }

    /// <summary>
    /// pc上级id.
    /// </summary>
    public List<string> pcModuleParentId { get; set; }

    /// <summary>
    /// app系统id.
    /// </summary>
    public string appSystemId { get; set; }

    /// <summary>
    /// pc系统id.
    /// </summary>
    public string pcSystemId { get; set; }
}
