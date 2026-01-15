namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新价值链状态.
/// </summary>
[SuppressSniffer]
public class MetricCovStatusUpInput : MetricCovStatusCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}