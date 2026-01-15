using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys;

/// <summary>
/// 消息接收
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_MESSAGERECEIVE")]
public class MessageReceiveEntity : OEntityBase<string>
{
    /// <summary>
    /// 消息主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_MESSAGEID")]
    public string MessageId { get; set; }

    /// <summary>
    /// 用户主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERID")]
    public string UserId { get; set; }

    /// <summary>
    /// 是否阅读.
    /// </summary>
    [SugarColumn(ColumnName = "F_ISREAD")]
    public int? IsRead { get; set; }

    /// <summary>
    /// 阅读时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_READTIME")]
    public DateTime? ReadTime { get; set; }

    /// <summary>
    /// 阅读次数.
    /// </summary>
    [SugarColumn(ColumnName = "F_READCOUNT")]
    public int? ReadCount { get; set; }

    /// <summary>
    /// 正文.
    /// </summary>
    [SugarColumn(ColumnName = "F_BODYTEXT")]
    public string BodyText { get; set; }

    /// <summary>
    /// 摘要.
    /// </summary>
    [SugarColumn(ColumnName = "F_Excerpt")]
    public string Excerpt { get; set; }
}