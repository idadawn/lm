namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标分级.
/// </summary>
[SuppressSniffer]
public class MetricGradedUpInput : MetricGradedCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}