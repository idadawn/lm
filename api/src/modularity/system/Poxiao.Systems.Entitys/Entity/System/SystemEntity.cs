using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.System;

/// <summary>
/// 系统功能.
/// </summary>
[SugarTable("BASE_SYSTEM")]
public class SystemEntity : CLDEntityBase
{
    /// <summary>
    /// 系统名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 系统编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 系统图标.
    /// </summary>
    [SugarColumn(ColumnName = "F_ICON")]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 是否是主系统（0-不是，1-是）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISMAIN")]
    public int? IsMain { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROPERTYJSON")]
    public string PropertyJson { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}
