namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标标签信息.
/// </summary>
[SuppressSniffer]
public class MetricTagsInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

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
    /// 是否删除.
    /// </summary>
    [JsonProperty("isDeleted")]
    public int IsDeleted { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 租户id.
    /// </summary>
    [JsonProperty("tenantId")]
    public string? TenantId { get; set; }

}