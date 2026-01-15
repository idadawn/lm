namespace Poxiao.Kpi.Application;

/// <summary>
/// 创建消息通知.
/// </summary>
public class MetricNoticeCrInput
{
    /// <summary>
    /// 类型.
    /// </summary>
    [JsonProperty("type", Required = Required.Always)]
    [Required(ErrorMessage = "类型信息不能为空.")]
    public MetricNoticeType Type { get; set; }

    /// <summary>
    /// 价值链节点.
    /// </summary>
    [JsonProperty("nodeId", Required = Required.Always)]
    [Required(ErrorMessage = "节点信息不能为空.")]
    public string NodeId { get; set; }

    /// <summary>
    /// 价值链节点的规则.
    /// </summary>
    [JsonProperty("ruleId")]
    public string? RuleId { get; set; }

    /// <summary>
    /// 模板.
    /// </summary>
    [JsonProperty("templateId", Required = Required.Always)]
    [Required(ErrorMessage = "消息模版不能为空.")]
    public string TemplateId { get; set; }
}