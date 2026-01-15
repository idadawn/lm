namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标仪表板.
/// </summary>
[SuppressSniffer]
public class MetricDashUpInput : MetricDashCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}