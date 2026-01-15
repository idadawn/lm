using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;

namespace Poxiao.Message.Entitys.Dto.Message;

/// <summary>
/// 消息公告输入.
/// </summary>
[SuppressSniffer]
public class MessageNoticeQueryInput : PageInputBase
{
    /// <summary>
    /// 状态(0-存草稿，1-已发布，2-已过期).
    /// </summary>
    public List<string> enabledMark { get; set; } = new List<string>();

    /// <summary>
    /// 类型.
    /// </summary>
    public List<string> type { get; set; } = new List<string>();
}