namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标价值链信息.
/// </summary>
[SuppressSniffer]
public class MetricCovInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 价值链名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 思维图类型.
    /// </summary>
    [JsonProperty("gotType")]
    public string GotType { get; set; }

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
    /// 价值链拼接.
    /// </summary>
    [JsonProperty("covTreeId")]
    public string? CovTreeId { get; set; }

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

    /// <summary>
    /// 价值链拼接.
    /// </summary>
    [JsonProperty("gotTreeId")]
    public string? GotTreeId { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonProperty("createdTime")]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 创建人.
    /// </summary>
    [JsonProperty("createdUserid")]
    public string? CreatedUserid { get; set; }

    /// <summary>
    /// 最后修改时间.
    /// </summary>
    [JsonProperty("lastModifiedTime")]
    public DateTime? LastModifiedTime { get; set; }

    /// <summary>
    /// 最后修改人.
    /// </summary>
    [JsonProperty("lastModifiedUserid")]
    public string? LastModifiedUserid { get; set; }

    /// <summary>
    /// 租户Id.
    /// </summary>
    [JsonProperty("tenantId")]
    public string? TenantId { get; set; }

}