namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标价值链.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
[SugarTable("metric_cov", TableDescription = "指标价值链")]
public class MetricCovEntity : CUEntityBase
{
    /// <summary>
    /// 价值链名称.
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "价值链名称")]
    public string Name { get; set; }

    /// <summary>
    /// 思维图类型.
    /// </summary>
    [SugarColumn(ColumnName = "got_type", ColumnDescription = "思维图类型", SqlParameterDbType = typeof(EnumToStringConvert))]
    public GotType GotType { get; set; }

    /// <summary>
    /// 思维图id.
    /// </summary>
    [SugarColumn(ColumnName = "got_id", ColumnDescription = "思维图id")]
    public string GotId { get; set; }

    /// <summary>
    /// 指标id.
    /// </summary>
    [SugarColumn(ColumnName = "metric_id", ColumnDescription = "指标id")]
    public string? MetricId { get; set; }

    /// <summary>
    /// 父级.
    /// </summary>
    [SugarColumn(ColumnName = "parent_id", ColumnDescription = "父级")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 价值链拼接.
    /// </summary>
    [SugarColumn(ColumnName = "cov_tree_id", ColumnDescription = "价值链拼接")]
    public string? CovTreeId { get; set; }

    /// <summary>
    /// 是否根节点.
    /// </summary>
    [SugarColumn(ColumnName = "is_root", ColumnDescription = "是否根节点")]
    public bool IsRoot { get; set; }

    /// <summary>
    /// 父级.
    /// </summary>
    [SugarColumn(ColumnName = "got_parent_id", ColumnDescription = "父级")]
    public string? GotParentId { get; set; }

    /// <summary>
    /// 价值链拼接.
    /// </summary>
    [SugarColumn(ColumnName = "got_tree_id", ColumnDescription = "价值链拼接")]
    public string? GotTreeId { get; set; }
}