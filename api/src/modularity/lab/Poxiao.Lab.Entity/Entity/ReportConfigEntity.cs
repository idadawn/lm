using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 报表统计配置实体
/// </summary>
[SugarTable("LAB_REPORT_CONFIG")]
[Tenant(ClaimConst.TENANTID)]
public class ReportConfigEntity : CLDEntityBase
{
    /// <summary>
    /// 统计名称 (如: "合格率", "A类占比")
    /// </summary>
    [SugarColumn(ColumnName = "F_NAME", Length = 100, IsNullable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 包含的判定等级名称列表 (JSON数组字符串)
    /// </summary>
    [SugarColumn(ColumnName = "F_LEVEL_NAMES", Length = 2000, IsNullable = true)]
    public string LevelNames { get; set; }

    /// <summary>
    /// 是否为系统默认配置 (1=是, 0=否)
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_SYSTEM")]
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// 排序号
    /// </summary>
    [SugarColumn(ColumnName = "F_SORT_ORDER")]
    public int SortOrder { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION", Length = 500, IsNullable = true)]
    public string Description { get; set; }
    /// <summary>
    /// 是否在头部展示
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_HEADER")]
    public bool IsHeader { get; set; } = false;

    /// <summary>
    /// 是否百分比展示
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_PERCENTAGE")]
    public bool IsPercentage { get; set; } = false;

    /// <summary>
    /// 是否报表中展示
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_SHOW_IN_REPORT")]
    public bool IsShowInReport { get; set; } = true;

    /// <summary>
    /// 是否显示占比
    /// </summary>
    [SugarColumn(ColumnName = "F_IS_SHOW_RATIO")]
    public bool IsShowRatio { get; set; } = true;

    /// <summary>
    /// 判定公式ID
    /// </summary>
    [SugarColumn(ColumnName = "F_FORMULA_ID", IsNullable = true)]
    public string FormulaId { get; set; }
}
