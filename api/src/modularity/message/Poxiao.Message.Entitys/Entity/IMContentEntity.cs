using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys;

/// <summary>
/// 在线聊天
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_IMCONTENT")]
public class IMContentEntity : OEntityBase<string>
{
    /// <summary>
    /// 发送者.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_SENDUSERID")]
    public string SendUserId { get; set; }

    /// <summary>
    /// 发送时间.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_SENDTIME")]
    public DateTime? SendTime { get; set; }

    /// <summary>
     /// 接收者.
     /// </summary>
     /// <returns></returns>
    [SugarColumn(ColumnName = "F_RECEIVEUSERID")]
    public string ReceiveUserId { get; set; }

    /// <summary>
    /// 接收时间.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_RECEIVETIME")]
    public DateTime? ReceiveTime { get; set; }

    /// <summary>
    /// 内容.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_CONTENT")]
    public string Content { get; set; }

    /// <summary>
    /// 内容类型：text、img、file.
    /// </summary>
    [SugarColumn(ColumnName = "F_CONTENTTYPE")]
    public string ContentType { get; set; }

    /// <summary>
    /// 状态（0:未读、1：已读）.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_STATE")]
    public int? State { get; set; }

    /// <summary>
    /// 删除人.
    /// </summary>
    [SugarColumn(ColumnName = "F_SENDDELETEMARK")]
    public string SendDeleteMark { get; set; }
}