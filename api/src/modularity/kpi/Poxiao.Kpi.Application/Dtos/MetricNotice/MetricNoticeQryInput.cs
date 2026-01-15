namespace Poxiao.Kpi.Application;

/// <summary>
/// 消息通知返回.
/// </summary>
public class MetricNoticeQryInput
{
    /// <summary>
    /// 节点.
    /// </summary>
    [JsonProperty("nodeId")]
    public string? NodeId { get; set; }

    /// <summary>
    /// 规则.
    /// </summary>
    [JsonProperty("ruleId")]
    public string? RuleId { get; set; }

    /// <summary>
    /// 当前页码:pageIndex.
    /// </summary>
    [JsonProperty("currentPage")]
    public virtual int CurrentPage { get; set; } = 1;

    /// <summary>
    /// 每页行数.
    /// </summary>
    [JsonProperty("pageSize")]
    public virtual int PageSize { get; set; } = 50;
}
