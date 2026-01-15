namespace Poxiao.Kpi.Application;

/// <summary>
/// 公共维度列表信息.
/// </summary>
[SuppressSniffer]
public class MetricDimensionListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 模型类别.
    /// </summary>
    [JsonProperty("dateModelType")]
    public string DateModelType { get; set; }

    /// <summary>
    /// 模型id.
    /// </summary>
    [JsonProperty("dataModelId")]
    public string DataModelId { get; set; }

    /// <summary>
    /// 维度名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    [JsonProperty("dataType")]
    public string DataType { get; set; }

    /// <summary>
    /// 列.
    /// </summary>
    [JsonProperty("column")]
    public string Column { get; set; }

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