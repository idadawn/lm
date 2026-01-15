namespace Poxiao.Infrastructure.Models;

[SuppressSniffer]
public class ScheduleTaskModel
{
    /// <summary>
    /// 定时任务请求参数实体.
    /// </summary>
    public Dictionary<string, object> taskParams { get; set; } = new Dictionary<string, object>();
}