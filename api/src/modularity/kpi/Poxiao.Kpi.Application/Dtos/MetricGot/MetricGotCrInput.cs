namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建指标思维图.
/// </summary>
[SuppressSniffer]
public class MetricGotCrInput
{
    /// <summary>
    /// 思维图类型.
    /// </summary>
    [JsonProperty("type")]
    public GotType Type { get; set; }

    /// <summary>
    /// 价值链名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 图片名称.
    /// </summary>
    [JsonProperty("imgName")]
    public string ImgName { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    [JsonProperty("metricTag")]
    public string? MetricTag { get; set; }

}