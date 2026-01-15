namespace Poxiao.Kpi.Application;

/// <summary>
/// 消息通知模板.
/// </summary>
public class MetricNoticeTemplateOutput
{
    /// <summary>
    /// id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 模板名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    [JsonProperty("enCode")]
    public string? EnCode { get; set; }

    /// <summary>
    /// 模板类型.
    /// </summary>
    [JsonProperty("templateType")]
    public string? TemplateType { get; set; }

    /// <summary>
    /// 消息来源.
    /// </summary>
    [JsonProperty("messageSource")]
    public string? MessageSource { get; set; }

    /// <summary>
    /// 消息类型.
    /// </summary>
    [JsonProperty("messageType")]
    public string? MessageType { get; set; }
}
