namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建标签.
/// </summary>
[SuppressSniffer]
public class MetricTagCrInput
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

    /// <summary>
    /// 描述.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

}
