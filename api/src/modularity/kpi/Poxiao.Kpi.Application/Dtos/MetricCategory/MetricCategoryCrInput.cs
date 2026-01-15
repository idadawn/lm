namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建指标分类.
/// </summary>
[SuppressSniffer]
public class MetricCategoryCrInput
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
    /// 父级.
    /// </summary>
    [JsonProperty("parentId")]
    public string ParentId { get; set; } = "-1";

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