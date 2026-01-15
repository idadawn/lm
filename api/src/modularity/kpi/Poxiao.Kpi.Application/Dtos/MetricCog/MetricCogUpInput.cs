namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标图链.
/// </summary>
[SuppressSniffer]
public class MetricCogUpInput : MetricCogCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}