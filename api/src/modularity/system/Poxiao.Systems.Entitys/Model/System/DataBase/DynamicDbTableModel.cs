using Poxiao.DependencyInjection;

namespace Poxiao.Systems.Entitys.Model.DataBase;

/// <summary>
/// 表列表实体.
/// </summary>
[SuppressSniffer]
public class DynamicDbTableModel
{
    /// <summary>
    /// 表名.
    /// </summary>
    public string? FTABLE { get; set; }

    /// <summary>
    /// 表说明.
    /// </summary>
    public string? FTABLENAME { get; set; }

    /// <summary>
    /// 大小.
    /// </summary>
    public string? FSIZE { get; set; }

    /// <summary>
    /// 总数.
    /// </summary>
    public string? FSUM { get; set; }

    /// <summary>
    /// 主键.
    /// </summary>
    public string? FPRIMARYKEY { get; set; }
}