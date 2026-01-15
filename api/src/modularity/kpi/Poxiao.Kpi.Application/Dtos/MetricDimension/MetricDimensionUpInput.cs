namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新公共维度.
/// </summary>
[SuppressSniffer]
public class MetricDimensionUpInput : MetricDimensionCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}