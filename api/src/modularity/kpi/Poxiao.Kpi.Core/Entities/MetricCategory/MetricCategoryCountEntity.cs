namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标分类计数.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-22.
/// </summary>
[SugarTable("metric_category_count", TableDescription = "指标分类计数")]
public class MetricCategoryCountEntity :CUDEntityBase
{
    /// <summary>
    /// 指标分类名称
    /// </summary>
    [SugarColumn(ColumnName = "category_id", ColumnDescription = "指标分类名称")]
    public string CategoryId { get; set; }

    /// <summary>
    /// 指标id
    /// </summary>
    [SugarColumn(ColumnName = "metric_id", ColumnDescription = "指标id")]
    public string MetricId { get; set; }
}