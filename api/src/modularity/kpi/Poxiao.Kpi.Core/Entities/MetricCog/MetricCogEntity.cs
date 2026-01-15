namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标图链.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
[SugarTable("metric_cog", TableDescription = "指标图链")]
public class MetricCogEntity : CUDEntityBase
{
    /// <summary>
    /// 指标id.
    /// </summary>
    [SugarColumn(ColumnName = "metric_id", ColumnDescription = "指标id")]
    public string? MetricId { get; set; }

    /// <summary>
    /// 父级指标.
    /// </summary>
    [SugarColumn(ColumnName = "parent_id", ColumnDescription = "父级指标")]
    public long? ParentId { get; set; }

    /// <summary>
    /// 图形链.
    /// </summary>
    [SugarColumn(ColumnName = "chain_of_graph_ids", ColumnDescription = "图形链")]
    public string? ChainOfGraphIds { get; set; }
}