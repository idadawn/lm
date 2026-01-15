using Poxiao.DependencyInjection;

namespace Poxiao.TaskScheduler.Entitys.Dto.TaskScheduler;

[SuppressSniffer]
public class TimeTaskUpInput:TimeTaskCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}

