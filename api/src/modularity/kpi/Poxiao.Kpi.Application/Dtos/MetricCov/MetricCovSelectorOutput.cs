namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分类树状结构.
/// </summary>
[SuppressSniffer]
public class MetricCovSelectorOutput : TreeModel
{
    /// <summary>
    /// 标签名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 思维图id.
    /// </summary>
    [JsonProperty("gotId")]
    public string GotId { get; set; }

}
