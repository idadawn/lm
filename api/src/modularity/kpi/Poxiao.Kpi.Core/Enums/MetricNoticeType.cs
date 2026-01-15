namespace Poxiao.Kpi.Core.Enums;

/// <summary>
/// 消息提示类型.
/// </summary>
[JsonConverter(typeof(EnumUseNameConverter<MetricNoticeType>))]
public enum MetricNoticeType
{
    /// <summary>
    /// 节点.
    /// </summary>
    [Description("节点")]
    Node = 0,

    /// <summary>
    /// 规则.
    /// </summary>
    [Description("规则")]
    Rule = 1
}
