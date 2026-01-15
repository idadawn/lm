using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.System;

/// <summary>
/// 门户管理.
/// </summary>
[SugarTable("BASE_PORTAL_MANAGE")]
public class PortalManageEntity : CLDEntityBase
{
    /// <summary>
    /// 说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 门户id.
    /// </summary>
    [SugarColumn(ColumnName = "F_PORTAL_ID")]
    public string PortalId { get; set; }

    /// <summary>
    /// 系统id.
    /// </summary>
    [SugarColumn(ColumnName = "F_SYSTEM_ID")]
    public string SystemId { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; } = 0;

    /// <summary>
    /// Web:网页端 App:手机端.
    /// </summary>
    [SugarColumn(ColumnName = "F_PLATFORM")]
    public string Platform { get; set; }
}
