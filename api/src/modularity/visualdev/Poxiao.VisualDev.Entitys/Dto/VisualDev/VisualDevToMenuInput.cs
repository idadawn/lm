namespace Poxiao.VisualDev.Entitys.Dto.VisualDev;

/// <summary>
/// 可视化开发同步到菜单输入.
/// </summary>
public class VisualDevToMenuInput
{
    /// <summary>
    /// ID.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 同步App菜单 1 同步.
    /// </summary>
    public int app { get; set; } = 1;

    /// <summary>
    /// 同步PC菜单 1 同步.
    /// </summary>
    public int pc { get; set; } = 1;

    /// <summary>
    /// Pc端同步菜单父级ID.
    /// </summary>
    public string? pcModuleParentId { get; set; }

    /// <summary>
    /// App端同步菜单父级ID.
    /// </summary>
    public string? appModuleParentId { get; set; }

    /// <summary>
    /// pc系统id.
    /// </summary>
    public string? pcSystemId { get; set; } = string.Empty;

    /// <summary>
    /// app系统id.
    /// </summary>
    public string? appSystemId { get; set; } = string.Empty;
}