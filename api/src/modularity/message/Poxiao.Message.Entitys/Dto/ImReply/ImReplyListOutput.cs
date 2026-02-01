using Poxiao.DependencyInjection;
using System.Text.Json.Serialization;

namespace Poxiao.Message.Entitys.Dto.ImReply;

/// <summary>
/// 聊天会话列表输出.
/// </summary>
[SuppressSniffer]
public class ImReplyListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string Id;

    /// <summary>
    /// 发送者.
    /// </summary>
    [JsonIgnore]
    public string sendUserId { get; set; }

    /// <summary>
    /// 接受者.
    /// </summary>
    [JsonIgnore]
    public string UserId;

    /// <summary>
    /// 名称.
    /// </summary>
    public string RealName;

    /// <summary>
    /// 头像.
    /// </summary>
    public string HeadIcon;

    /// <summary>
    /// 最新消息.
    /// </summary>
    public string LatestMessage;

    /// <summary>
    /// 最新时间.
    /// </summary>
    public DateTime LatestDate;

    /// <summary>
    /// 未读消息.
    /// </summary>
    public int UnreadMessage;

    /// <summary>
    /// 消息类型.
    /// </summary>
    public string MessageType;

    /// <summary>
    /// 账号.
    /// </summary>
    public string Account;
}