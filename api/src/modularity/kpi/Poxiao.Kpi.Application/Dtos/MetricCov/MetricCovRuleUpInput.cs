namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标价值链规则.
/// </summary>
[SuppressSniffer]
public class MetricCovRuleUpInput : MetricCovRuleCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}