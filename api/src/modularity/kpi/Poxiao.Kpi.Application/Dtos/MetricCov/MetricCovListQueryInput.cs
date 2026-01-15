namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标价值链查询信息.
/// </summary>
[SuppressSniffer]
public class MetricCovListQueryInput
{
    /// <summary>
    /// 思维图id.
    /// </summary>
    [JsonProperty("gotId")]
    public long GotId { get; set; }
}