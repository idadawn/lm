using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.System;

/// <summary>
/// 日程日志.
/// </summary>
[SugarTable("BASE_SCHEDULE_LOG")]
public class ScheduleLogEntity : CLDEntityBase
{
    /// <summary>
    /// 类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_Type")]
    public string Type { get; set; }

    /// <summary>
    /// 紧急程度.
    /// </summary>
    [SugarColumn(ColumnName = "F_Urgent")]
    public string Urgent { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    [SugarColumn(ColumnName = "F_Title")]
    public string Title { get; set; }

    /// <summary>
    /// 内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_Content")]
    public string Content { get; set; }

    /// <summary>
    /// 全天.
    /// </summary>
    [SugarColumn(ColumnName = "F_AllDay")]
    public int AllDay { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_StartDay")]
    public DateTime StartDay { get; set; }

    /// <summary>
    /// 开始日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_StartTime")]
    public string StartTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_EndDay")]
    public DateTime EndDay { get; set; }

    /// <summary>
    /// 结束日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_EndTime")]
    public string EndTime { get; set; }

    /// <summary>
    /// 时长.
    /// </summary>
    [SugarColumn(ColumnName = "F_Duration")]
    public int Duration { get; set; }

    /// <summary>
    /// 颜色.
    /// </summary>
    [SugarColumn(ColumnName = "F_Color")]
    public string Color { get; set; }

    /// <summary>
    /// 提醒.
    /// </summary>
    [SugarColumn(ColumnName = "F_ReminderTime")]
    public int ReminderTime { get; set; }

    /// <summary>
    /// 提醒方式.
    /// </summary>
    [SugarColumn(ColumnName = "F_ReminderType")]
    public string ReminderType { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_SortCode")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 发送配置.
    /// </summary>
    [SugarColumn(ColumnName = "F_Send")]
    public string Send { get; set; }

    /// <summary>
    /// 发送配置名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_SendName")]
    public string SendName { get; set; }

    /// <summary>
    /// 重复提醒.
    /// </summary>
    [SugarColumn(ColumnName = "F_Repetition")]
    public string Repetition { get; set; }

    /// <summary>
    /// 结束重复.
    /// </summary>
    [SugarColumn(ColumnName = "F_RepeatTime")]
    public DateTime? RepeatTime { get; set; }

    /// <summary>
    /// 推送时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_PushTime")]
    public DateTime? PushTime { get; set; }

    /// <summary>
    /// 分组id.
    /// </summary>
    [SugarColumn(ColumnName = "F_GroupId")]
    public string GroupId { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_Description")]
    public string Description { get; set; }

    /// <summary>
    /// 操作类型(1:新增，2：修改，3：删除，4：删除参与人).
    /// </summary>
    [SugarColumn(ColumnName = "F_OperationType")]
    public string OperationType { get; set; }

    /// <summary>
    /// 参与用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_UserId")]
    public string UserId { get; set; }

    /// <summary>
    /// 日程id.
    /// </summary>
    [SugarColumn(ColumnName = "F_ScheduleId")]
    public string ScheduleId { get; set; }
}
