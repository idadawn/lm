using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.System;

/// <summary>
/// 日程参与用户.
/// </summary>
[SugarTable("BASE_SCHEDULE_USER")]
public class ScheduleUserEntity : CLDEntityBase
{
    /// <summary>
    /// 日程id.
    /// </summary>
    [SugarColumn(ColumnName = "F_ScheduleId")]
    public string ScheduleId { get; set; }

    /// <summary>
    /// 用户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_ToUserIds")]
    public string ToUserIds { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_SortCode")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_Description")]
    public string Description { get; set; }

    /// <summary>
    /// 1.系统创建 2.用户创建.
    /// </summary>
    [SugarColumn(ColumnName = "F_Type")]
    public string Type { get; set; }
}
