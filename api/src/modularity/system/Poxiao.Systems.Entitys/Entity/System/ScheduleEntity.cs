using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.System;

/// <summary>
/// 日程.
/// </summary>
[SugarTable("BASE_SCHEDULE")]
public class ScheduleEntity : CLDEntityBase
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
    /// 提醒(-2:不提醒,-1:开始时,5:提前5分钟,10:提前10分钟,15:提前15分钟,30:提前30分钟,60:提前1小时,120:提前2小时,1440:提前1天).
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
    /// 重复提醒 1.不重复 2.每天重复 3.每周重复 4.每月重复 5.每年重复.
    /// </summary>
    [SugarColumn(ColumnName = "F_Repetition")]
    public string Repetition { get; set; }

    /// <summary>
    /// 结束重复时间.
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
    /// 日程参与人（一对多）.
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(ScheduleUserEntity.ScheduleId), nameof(Id))]
    public List<ScheduleUserEntity> ScheduleUser { get; set; }
}
