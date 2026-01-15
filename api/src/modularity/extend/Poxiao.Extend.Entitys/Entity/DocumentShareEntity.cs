using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Extend.Entitys;

/// <summary>
/// 知识文档共享
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
[SugarTable("EXT_DOCUMENTSHARE")]
public class DocumentShareEntity : OEntityBase<string>
{
    /// <summary>
    /// 文档主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_DOCUMENTID")]
    public string? DocumentId { get; set; }

    /// <summary>
    /// 共享人员.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHAREUSERID")]
    public string? ShareUserId { get; set; }

    /// <summary>
    /// 共享时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_SHARETIME")]
    public DateTime? ShareTime { get; set; }
}
