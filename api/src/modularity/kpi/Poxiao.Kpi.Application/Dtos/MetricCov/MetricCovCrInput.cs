namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建指标价值链.
/// </summary>
[SuppressSniffer]
public class MetricCovCrInput
{
    /// <summary>
    /// 价值链名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 思维图类型.
    /// </summary>
    [JsonProperty("gotType")]
    public GotType GotType { get; set; }

    /// <summary>
    /// 思维图id.
    /// </summary>
    [JsonProperty("gotId")]
    public string GotId { get; set; }

    /// <summary>
    /// 指标id.
    /// </summary>
    [JsonProperty("metricId")]
    public string? MetricId { get; set; }

    /// <summary>
    /// 父级.
    /// </summary>
    [JsonProperty("parentId")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 是否根节点.
    /// </summary>
    [JsonProperty("is_root")]
    public bool IsRoot { get; set; }

    /// <summary>
    /// 父级.
    /// </summary>
    [JsonProperty("gotParentId")]
    public string? GotParentId { get; set; }
}