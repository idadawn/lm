using Poxiao.Infrastructure.Filter;

namespace Poxiao.Message.Entitys.Dto.MessageMonitor;

public class MessageMonitorQuery : PageInputBase
{
    /// <summary>
    /// 开始时间.
    /// </summary>
    public long? startTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    public long? endTime { get; set; }

    /// <summary>
    /// 消息类型.
    /// </summary>
    public string? messageType { get; set; }

    /// <summary>
    /// 消息来源.
    /// </summary>
    public string? messageSource { get; set; }
}
