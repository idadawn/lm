namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标标签计数.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-22.
/// </summary>
[SugarTable("metric_tags_count", TableDescription = "指标标签计数")]
public class MetricTagsCountEntity : CUDEntityBase
{
    /// <summary>
    /// 指标id
    /// </summary>
    [SugarColumn(ColumnName = "tag_id", ColumnDescription = "指标id")]
    public string TagId { get; set; }

    /// <summary>
    /// 指标计数分类
    /// </summary>
    [SugarColumn(ColumnName = "tag_count_category", ColumnDescription = "指标计数分类")]
    public string TagCountCategory { get; set; }

    /// <summary>
    /// 关联id
    /// </summary>
    [SugarColumn(ColumnName = "relation_id", ColumnDescription = "关联id")]
    public string RelationId { get; set; }
}