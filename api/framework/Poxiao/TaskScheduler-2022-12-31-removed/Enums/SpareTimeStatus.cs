using System.ComponentModel;

namespace Poxiao.TaskScheduler;

/// <summary>
/// 任务状态
/// </summary>
/// <remarks>TaskScheduler 将于2022年12月31日移除，请使用 Scheduler 替代</remarks>
[Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]
[SuppressSniffer]
public enum SpareTimeStatus
{
    /// <summary>
    /// 运行中
    /// </summary>
    [Description("运行中")]
    Running,

    /// <summary>
    /// 已停止或未启动
    /// </summary>
    [Description("已停止或未启动")]
    Stopped,

    /// <summary>
    /// 单次执行失败
    /// </summary>
    [Description("任务停止并失败")]
    Failed,

    /// <summary>
    /// 任务已取消或没有该任务
    /// </summary>
    [Description("任务已取消或没有该任务")]
    CanceledOrNone
}