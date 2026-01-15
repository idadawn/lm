namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标分类.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-22.
/// </summary>
[SugarTable("metric_category", TableDescription = "指标分类")]
public class MetricCategoryEntity : CUDEntityBase
{
    /// <summary>
    /// 标签名称
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "标签名称")]
    public string Name { get; set; }

    /// <summary>
    /// 排序码
    /// </summary>
    [SugarColumn(ColumnName = "sort", ColumnDescription = "排序码")]
    public long Sort { get; set; }

    /// <summary>
    /// 所有者
    /// </summary>
    [SugarColumn(ColumnName = "own_id", ColumnDescription = "所有者")]
    public string OwnId { get; set; }

    /// <summary>
    /// 父级
    /// </summary>
    [SugarColumn(ColumnName = "parent_id", ColumnDescription = "父级")]
    public string ParentId { get; set; }

    /// <summary>
    /// 分类拼接
    /// </summary>
    [SugarColumn(ColumnName = "category_id_tree", ColumnDescription = "分类拼接")]
    public string CategoryIdTree { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "description", ColumnDescription = "描述")]
    public string? Description { get; set; }
}