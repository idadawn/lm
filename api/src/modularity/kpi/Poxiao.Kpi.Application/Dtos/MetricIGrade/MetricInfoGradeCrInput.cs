namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建指标分级.
/// </summary>
[SuppressSniffer]
public class MetricGradedCrInput
{
    /// <summary>
    /// 指标id.
    /// </summary>
    [JsonProperty("metricId")]
    public string MetricId { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 类别.
    /// </summary>
    [JsonProperty("type")]
    public MetricGradeType Type { get; set; }

    /// <summary>
    /// 区间类型.
    /// </summary>
    [JsonProperty("rangType")]
    public CovRuleValueType RangType { get; set; } = CovRuleValueType.Value;

    /// <summary>
    /// 趋势.
    /// </summary>
    [JsonProperty("trend")]
    public TrendType? Trend { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    [JsonProperty("value")]
    public decimal Value { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// 状态颜色.
    /// </summary>
    [JsonProperty("status_color")]
    public string StatusColor { get; set; }

}