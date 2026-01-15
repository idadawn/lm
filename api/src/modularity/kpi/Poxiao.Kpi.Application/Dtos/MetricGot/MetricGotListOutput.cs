namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标思维图列表信息.
/// </summary>
[SuppressSniffer]
public class MetricGotListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 思维图类型.
    /// </summary>
    [JsonProperty("type")]
    public GotType? Type { get; set; }

    /// <summary>
    /// 思维图类型.
    /// </summary>
    [JsonProperty("typeStr")]
    public string? TypeStr { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [JsonProperty("sort")]
    public long? Sort { get; set; }

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

    /// <summary>
    /// 标签名称
    /// </summary>
    [JsonProperty("metricTagName")]
    public List<string> MetricTagNames { get; set; } = new List<string>();
}