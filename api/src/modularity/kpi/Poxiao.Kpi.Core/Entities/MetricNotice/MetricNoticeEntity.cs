namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标通知.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2023-11-28
/// </summary>
[SugarTable("metric_notice", TableDescription = "指标通知")]
public class MetricNoticeEntity : CUEntityBase
{
    /// <summary>
    /// 类型.
    /// </summary>
    [SugarColumn(ColumnName = "type", ColumnDescription = "类型", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MetricNoticeType Type { get; set; }

    /// <summary>
    /// 价值链节点.
    /// </summary>
    [SugarColumn(ColumnName = "node_id", ColumnDescription = "价值链节点")]
    public string NodeId { get; set; }

    /// <summary>
    /// 规则信息.
    /// </summary>
    [SugarColumn(ColumnName = "rule_id", ColumnDescription = "规则信息")]
    public string? RuleId { get; set; }

    /// <summary>
    /// 模板信息.
    /// </summary>
    [SugarColumn(ColumnName = "template_id", ColumnDescription = "模板信息")]
    public string TemplateId { get; set; }

    /// <summary>
    /// 调度信息.
    /// </summary>
    [SugarColumn(ColumnName = "schedule_id", ColumnDescription = "调度信息")]
    public string? ScheduleId { get; set; }
}