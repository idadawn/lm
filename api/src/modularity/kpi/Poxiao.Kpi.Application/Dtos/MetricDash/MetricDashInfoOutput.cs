namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标仪表板信息.
/// </summary>
[SuppressSniffer]
public class MetricDashInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 思维图id.
    /// </summary>
    [JsonProperty("gotId")]
    public string? GotId { get; set; }

    /// <summary>
    /// 思维图类别.
    /// </summary>
    [JsonProperty("gotType")]
    public string? GotType { get; set; }

    /// <summary>
    /// 表单数据.
    /// </summary>
    [JsonProperty("formJson")]
    public string? FormJson { get; set; }

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