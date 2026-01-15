namespace Poxiao.Kpi.Application;

/// <summary>
/// 新增复合指标定义信息.
/// </summary>
[SuppressSniffer]
public class MetricInfo4CompositeUpInput : MetricInfo4CompositeCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}
