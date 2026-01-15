namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标标签.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-10.
/// </summary>
[SugarTable("metric_tags", TableDescription = "指标标签")]
public class MetricTagsEntity : CUDEntityBase
{
    /// <summary>
    /// 标签名称.
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "标签名称")]
    public string Name { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "sort", ColumnDescription = "排序码")]
    public long Sort { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "description", ColumnDescription = "描述")]
    public string? Description { get; set; }

}