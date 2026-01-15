namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标价值链规则信息.
/// </summary>
[SuppressSniffer]
public class MetricCovRuleInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 价值链id.
    /// </summary>
    [JsonProperty("covId")]
    public string CovId { get; set; }

    /// <summary>
    /// 级别.
    /// </summary>
    [JsonProperty("level")]
    public string Level { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    [SugarColumn(ColumnName = "type", ColumnDescription = "类型")]
    public CovRuleValueType Type { get; set; }

    /// <summary>
    /// 操作符.
    /// </summary>
    [SugarColumn(ColumnName = "operators", ColumnDescription = "操作符")]
    public CovRuleOperatorsType Operators { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    [SugarColumn(ColumnName = "value", ColumnDescription = "值")]
    public decimal? Value { get; set; }

    /// <summary>
    /// 最小值.
    /// </summary>
    [JsonProperty("minValue")]
    public decimal? MinValue { get; set; }

    /// <summary>
    /// 最大值.
    /// </summary>
    [JsonProperty("maxValue")]
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; }

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