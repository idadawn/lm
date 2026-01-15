using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Extend.Entitys;

/// <summary>
/// 工作日志分享
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
[SugarTable("EXT_WORKLOGSHARE")]
public class WorkLogShareEntity : OEntityBase<string>
{
    /// <summary>
    /// 日志主键.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_WORKLOGID")]
    public string? WorkLogId { get; set; }

    /// <summary>
    /// 共享人员.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_SHAREUSERID")]
    public string? ShareUserId { get; set; }

    /// <summary>
    /// 共享时间.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_SHARETIME")]
    public DateTime? ShareTime { get; set; }
}
