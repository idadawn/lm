namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标分级.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2024-1-5
/// </summary>
[SugarTable("metric_graded", TableDescription = "指标分级")]
public class MetricGradedEntity : CUEntityBase
{
    /// <summary>
    /// 指标id.
    /// </summary>
    [SugarColumn(ColumnName = "metric_id", ColumnDescription = "指标id")]
    public string MetricId { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "name", ColumnDescription = "名称")]
    public string Name { get; set; }

    /// <summary>
    /// 类别.
    /// </summary>
    [SugarColumn(ColumnName = "type", ColumnDescription = "类别", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MetricGradeType Type { get; set; }

    /// <summary>
    /// 区间类型.
    /// </summary>
    [SugarColumn(ColumnName = "rang_type", ColumnDescription = "区间类型", SqlParameterDbType = typeof(EnumToStringConvert))]
    public CovRuleValueType? RangType { get; set; }

    /// <summary>
    /// 趋势.
    /// </summary>
    [SugarColumn(ColumnName = "trend", ColumnDescription = "趋势", SqlParameterDbType = typeof(EnumToStringConvert))]
    public TrendType? Trend { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    [SugarColumn(ColumnName = "value", ColumnDescription = "值")]
    public decimal Value { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "status", ColumnDescription = "状态")]
    public string Status { get; set; }

    /// <summary>
    /// 状态颜色.
    /// </summary>
    [SugarColumn(ColumnName = "status_color", ColumnDescription = "状态颜色")]
    public string StatusColor { get; set; }

}