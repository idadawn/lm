using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.WorkFlow.Entitys.Entity;

/// <summary>
/// 流程任务.
/// </summary>
[SugarTable("FLOW_TASK")]
public class FlowTaskEntity : CLDEntityBase
{
    /// <summary>
    /// 父级id.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 实例进程id.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROCESSID")]
    public string? ProcessId { get; set; }

    /// <summary>
    /// 任务编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string? EnCode { get; set; }

    /// <summary>
    /// 任务标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string? FullName { get; set; }

    /// <summary>
    /// 紧急程度.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWURGENT")]
    public int? FlowUrgent { get; set; }

    /// <summary>
    /// 流程主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWID")]
    public string? FlowId { get; set; }

    /// <summary>
    /// 流程编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWCODE")]
    public string? FlowCode { get; set; }

    /// <summary>
    /// 流程名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWNAME")]
    public string? FlowName { get; set; }

    /// <summary>
    /// 流程类型（0：发起流程，1：功能流程）.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWTYPE")]
    public int? FlowType { get; set; }

    /// <summary>
    /// 流程分类.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWCATEGORY")]
    public string? FlowCategory { get; set; }

    /// <summary>
    /// 流程表单.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWFORM")]
    public string? FlowForm { get; set; }

    /// <summary>
    /// 表单内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWFORMCONTENTJSON")]
    public string? FlowFormContentJson { get; set; }

    /// <summary>
    /// 流程模板.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWTEMPLATEJSON")]
    public string FlowTemplateJson { get; set; }

    /// <summary>
    /// 流程版本.
    /// </summary>
    [SugarColumn(ColumnName = "F_FLOWVERSION")]
    public string? FlowVersion { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_STARTTIME")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENDTIME")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 当前节点.
    /// </summary>
    [SugarColumn(ColumnName = "F_THISSTEP")]
    public string? ThisStep { get; set; }

    /// <summary>
    /// 当前节点编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_THISSTEPID")]
    public string? ThisStepId { get; set; }

    /// <summary>
    /// 重要等级(是否有恢复按钮 有值代表没有 无值代表有).
    /// </summary>
    [SugarColumn(ColumnName = "F_GRADE")]
    public string? Grade { get; set; }

    /// <summary>
    /// 任务状态：【0-草稿、1-处理、2-通过、3-驳回、4-撤销、5-终止】.
    /// </summary>
    [SugarColumn(ColumnName = "F_STATUS")]
    public int Status { get; set; } = 0;

    /// <summary>
    /// 完成情况(0:未完成，1:完成).
    /// </summary>
    [SugarColumn(ColumnName = "F_COMPLETION")]
    public int? Completion { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 同步异步（0：同步，1：异步）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISASYNC")]
    public int? IsAsync { get; set; }

    /// <summary>
    /// 是否批量（0：否，1：是）.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISBATCH")]
    public int? IsBatch { get; set; }

    /// <summary>
    /// 复活节点主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKNODEID")]
    public string? TaskNodeId { get; set; }

    /// <summary>
    /// 流程主表主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_TEMPLATEID")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// 拒绝节点id(当前节点审批).
    /// </summary>
    [SugarColumn(ColumnName = "F_REJECTDATAID")]
    public string? RejectDataId { get; set; }

    /// <summary>
    /// 委托发起人.
    /// </summary>
    [SugarColumn(ColumnName = "F_DELEGATEUSER")]
    public string? DelegateUser { get; set; }

    /// <summary>
    /// 挂起（0：否，1：是）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Suspend")]
    public int? Suspend { get; set; }
}
