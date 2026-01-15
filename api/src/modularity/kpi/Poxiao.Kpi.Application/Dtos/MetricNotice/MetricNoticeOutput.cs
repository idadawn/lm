namespace Poxiao.Kpi.Application;

/// <summary>
/// 消息通知信息.
/// </summary>
public class MetricNoticeOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    [JsonProperty("type")]
    public MetricNoticeType Type { get; set; }

    /// <summary>
    /// 节点.
    /// </summary>
    [JsonProperty("nodeId")]
    public string NodeId { get; set; }

    /// <summary>
    /// 规则.
    /// </summary>
    [JsonProperty("ruleId")]
    public string? RuleId { get; set; }

    /// <summary>
    /// 消息标题.
    /// </summary>
    [JsonProperty("noticeTitle")]
    public string NoticeTitle { get; set; }

    /// <summary>
    /// 模板.
    /// </summary>
    [JsonProperty("templateId")]
    public string TemplateId { get; set; }

    /// <summary>
    /// 调度信息.
    /// </summary>
    [JsonProperty("scheduleId")]
    public string? ScheduleId { get; set; }

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