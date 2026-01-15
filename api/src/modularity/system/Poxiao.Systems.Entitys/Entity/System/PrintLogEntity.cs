using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Systems.Entitys.Entity.System;

/// <summary>
/// 打印模板日志
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_PRINT_LOG")]
[Tenant(ClaimConst.TENANTID)]
public class PrintLogEntity : OEntityBase<string>
{
    /// <summary>
    /// 打印人.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrintMan")]
    public string PrintMan { get; set; }

    /// <summary>
    /// 打印时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrintTime")]
    public DateTime? PrintTime { get; set; }

    /// <summary>
    /// 打印条数.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrintNum")]
    public int? PrintNum { get; set; }

    /// <summary>
    /// 打印功能名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrintTitle")]
    public string PrintTitle { get; set; }

    /// <summary>
    /// 打印模板id.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrintId")]
    public string PrintId { get; set; }
}
