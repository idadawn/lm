using Poxiao.DependencyInjection;

namespace Poxiao.Message.Entitys.Dto.Message;

/// <summary>
/// 消息创建输入.
/// </summary>
[SuppressSniffer]
public class MessageCrInput
{
    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 正文内容.
    /// </summary>
    public string bodyText { get; set; }

    /// <summary>
    /// 收件用户.
    /// </summary>
    public string toUserIds { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public string files { get; set; }

    /// <summary>
    /// 分类.
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    public string coverImage { get; set; }

    /// <summary>
    /// 摘要.
    /// </summary>
    public string excerpt { get; set; }

    /// <summary>
    /// 过期时间.
    /// </summary>
    public DateTime? expirationTime { get; set; }

    /// <summary>
    /// 通知方式.
    /// </summary>
    public int? remindCategory { get; set; }

    /// <summary>
    /// 发送配置id.
    /// </summary>
    public string sendConfigId { get; set; }

    /// <summary>
    /// 发送配置名称.
    /// </summary>
    public string sendConfigName { get; set; }
}