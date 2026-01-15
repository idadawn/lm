namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标价值链.
/// </summary>
[SuppressSniffer]
public class MetricCovUpInput : MetricCovCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}