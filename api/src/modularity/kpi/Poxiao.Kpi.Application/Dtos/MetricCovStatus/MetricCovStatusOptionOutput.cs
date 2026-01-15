namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建价值链状态.
/// </summary>
[SuppressSniffer]
public class MetricCovStatusOptionOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 颜色.
    /// </summary>
    [JsonProperty("color")]
    public string Color { get; set; }

}

