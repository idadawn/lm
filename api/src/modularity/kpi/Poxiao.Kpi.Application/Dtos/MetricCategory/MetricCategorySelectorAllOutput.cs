namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分类树状结构.
/// </summary>
[SuppressSniffer]
public class MetricCategorySelectorAllOutput : TreeModel
{
    /// <summary>
    /// 标签名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [JsonProperty("sort")]
    public long Sort { get; set; }

}
