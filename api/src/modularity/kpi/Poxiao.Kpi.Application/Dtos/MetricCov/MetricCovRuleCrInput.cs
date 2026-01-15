namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建指标价值链规则.
/// </summary>
[SuppressSniffer]
public class MetricCovRuleCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 价值链id.
    /// </summary>
    [JsonProperty("covId")]
    public string CovId { get; set; }

    /// <summary>
    /// 级别.
    /// </summary>
    [JsonProperty("level")]
    public string Level { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    [JsonProperty("type")]
    public CovRuleValueType Type { get; set; } = CovRuleValueType.Value;

    /// <summary>
    /// 操作符.
    /// </summary>
    [JsonProperty("operators")]
    public CovRuleOperatorsType Operators { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    [JsonProperty("value")]
    public decimal Value { get; set; }

    /// <summary>
    /// 最小值.
    /// </summary>
    [JsonProperty("minValue")]
    public decimal? MinValue { get; set; }

    /// <summary>
    /// 最大值.
    /// </summary>
    [JsonProperty("maxValue")]
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; }

}