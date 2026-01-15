namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标定义.
/// </summary>
[SuppressSniffer]
public class MetricInfoUpInput : MetricInfoCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}