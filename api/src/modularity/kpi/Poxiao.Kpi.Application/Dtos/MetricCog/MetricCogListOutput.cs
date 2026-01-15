namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标图链列表信息.
/// </summary>
[SuppressSniffer]
public class MetricCogListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 指标id.
    /// </summary>
    [JsonProperty("metricId")]
    public string? MetricId { get; set; }

    /// <summary>
    /// 父级指标.
    /// </summary>
    [JsonProperty("parentId")]
    public long? ParentId { get; set; }

    /// <summary>
    /// 图形链.
    /// </summary>
    [JsonProperty("chainOfGraphIds")]
    public string? ChainOfGraphIds { get; set; }

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
    /// 删除时间.
    /// </summary>
    [JsonProperty("deleteTime")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除人.
    /// </summary>
    [JsonProperty("deleteUserid")]
    public string? DeleteUserid { get; set; }

    /// <summary>
    /// 租户Id.
    /// </summary>
    [JsonProperty("tenantId")]
    public string? TenantId { get; set; }

}