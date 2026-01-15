namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标思维图.
/// </summary>
[SuppressSniffer]
public class MetricGotUpInput : MetricGotCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}