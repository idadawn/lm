namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分级信息.
/// </summary>
[SuppressSniffer]
public class MetricGradedInfoOutput
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
    public string MetricId { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 类别.
    /// </summary>
    [JsonProperty("type")]
    public MetricGradeType Type { get; set; }

    /// <summary>
    /// 区间类型.
    /// </summary>
    [JsonProperty("rangType")]
    public CovRuleValueType? RangType { get; set; }

    /// <summary>
    /// 趋势.
    /// </summary>
    [JsonProperty("trend")]
    public TrendType? Trend { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// 状态颜色.
    /// </summary>
    [JsonProperty("status_color")]
    public string StatusColor { get; set; }

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