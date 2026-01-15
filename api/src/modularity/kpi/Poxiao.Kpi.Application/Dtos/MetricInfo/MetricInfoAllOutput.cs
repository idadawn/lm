namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标定义列表信息.
/// </summary>
[SuppressSniffer]
public class MetricInfoAllOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
    /// <summary>
    /// 指标名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}