using Poxiao.DependencyInjection;

namespace Poxiao.Message.Entitys.Dto.Message;

/// <summary>
/// 消息公告输出.
/// </summary>
[SuppressSniffer]
public class MessageNoticeOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 发布人员.
    /// </summary>
    public string releaseUser { get; set; }

    /// <summary>
    /// 发布时间.
    /// </summary>
    public DateTime? releaseTime { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 创建人员.
    /// </summary>
    public string creatorUser { get; set; }

    /// <summary>
    /// 状态(0-存草稿，1-已发布).
    /// </summary>
    public int? enabledMark { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    public int? type { get; set; }

    /// <summary>
    /// 分类.
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// 摘要.
    /// </summary>
    public string excerpt { get; set; }

    /// <summary>
    /// 过期时间.
    /// </summary>
    public DateTime? expirationTime { get; set; }
}