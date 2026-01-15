using System.ComponentModel;

namespace Poxiao.TaskScheduler;

/// <summary>
/// 任务类型
/// </summary>
/// <remarks>TaskScheduler 将于2022年12月31日移除，请使用 Scheduler 替代</remarks>
[Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]
[SuppressSniffer]
public enum SpareTimeTypes
{
    /// <summary>
    /// 间隔方式
    /// </summary>
    [Description("间隔方式")]
    Interval,

    /// <summary>
    /// Cron 表达式
    /// </summary>
    [Description("Cron 表达式")]
    Cron
}