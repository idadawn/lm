using SqlSugar.DbConvert;

namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分级列表信息.
/// </summary>
[SuppressSniffer]
public class MetricInfoGradeExtInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

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
    /// 值.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }

    /// <summary>
    /// 状态颜色.
    /// </summary>
    [JsonProperty("status_color")]
    public string StatusColor { get; set; }

    /// <summary>
    /// 是否显示.
    /// </summary>
    [JsonProperty("is_show")]
    public bool IsShow { get; set; }

    /// <summary>
    /// 区间类型.
    /// </summary>
    [JsonIgnore]
    public decimal DValue { get; set; }

    /// <summary>
    /// 区间类型.
    /// </summary>
    [JsonIgnore]
    public CovRuleValueType? RangType { get; set; }

}