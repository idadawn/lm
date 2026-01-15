namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标标签.
/// </summary>
[SuppressSniffer]
public class MetricTagUpInput : MetricTagCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}