namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标思维图查询信息.
/// </summary>
[SuppressSniffer]
public class MetricGotListQueryInput : PageInputBase
{
    /// <summary>
    /// 思维图类型.
    /// </summary>
    [JsonProperty("type")]
    protected GotType Type { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    [JsonProperty("metricTags")]
    public List<string>? MetricTags { get; set; }
}