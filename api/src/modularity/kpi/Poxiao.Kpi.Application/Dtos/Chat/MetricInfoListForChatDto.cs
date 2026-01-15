namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标信息.
/// </summary>
public class MetricInfoListForChatDto
{
    /// <summary>
    /// 指标id.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 指标名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}