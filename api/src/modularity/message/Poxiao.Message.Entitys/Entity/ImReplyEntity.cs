using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Message.Entitys;

/// <summary>
/// 聊天会话.
/// </summary>
[SugarTable("BASE_IMREPLY")]
public class ImReplyEntity : OEntityBase<string>
{
    /// <summary>
    /// 发送者.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_UserId")]
    public string UserId { get; set; }

    /// <summary>
    /// 接收用户.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ReceiveUserId")]
    public string ReceiveUserId { get; set; }

    /// <summary>
    /// 接收用户时间.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ReceiveTime")]
    public DateTime? ReceiveTime { get; set; }

    /// <summary>
    /// 删除人.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_IMREPLYSENDDELETEMARK")]
    public string ImreplySendDeleteMark { get; set; }
}