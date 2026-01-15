namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分类列表信息.
/// </summary>
[SuppressSniffer]
public class MetricCategoryListOutput : TreeModel
{
    /// <summary>
    /// 标签名称.
    /// </summary>
    [JsonProperty("fullName")]
    public string Name { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [JsonProperty("sort")]
    public long Sort { get; set; }

    /// <summary>
    /// 所有者.
    /// </summary>
    [JsonProperty("ownId")]
    public string OwnId { get; set; }

    /// <summary>
    /// 分类拼接
    /// </summary>
    [JsonProperty("organizeIdTree")]
    public string CategoryIdTree { get; set; }

    /// <summary>
    /// 分类Id.
    /// </summary>
    [JsonProperty("organizeIds")]
    public List<string> CategoryIds { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [JsonProperty("createdTime")]
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 创建人.
    /// </summary>
    [JsonProperty("createdUserid")]
    public string? CreatedUserId { get; set; }

    /// <summary>
    /// 最后修改时间.
    /// </summary>
    [JsonProperty("lastModifiedTime")]
    public DateTime? LastModifiedTime { get; set; }

    /// <summary>
    /// 最后修改人.
    /// </summary>
    [JsonProperty("lastModifiedUserid")]
    public string? LastModifiedUserId { get; set; }

    /// <summary>
    /// 删除时间.
    /// </summary>
    [JsonProperty("deleteTime")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除人.
    /// </summary>
    [JsonProperty("deleteUserid")]
    public string? DeleteUserId { get; set; }

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