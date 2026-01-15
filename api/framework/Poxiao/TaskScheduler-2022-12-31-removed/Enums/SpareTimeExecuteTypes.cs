using System.ComponentModel;

namespace Poxiao.TaskScheduler;

/// <summary>
/// 任务执行类型
/// </summary>
/// <remarks>TaskScheduler 将于2022年12月31日移除，请使用 Scheduler 替代</remarks>
[Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]
[SuppressSniffer]
public enum SpareTimeExecuteTypes
{
    /// <summary>
    /// 并行执行（默认方式）
    /// <para>无需等待上一个完成</para>
    /// </summary>
    [Description("并行执行")]
    Parallel,

    /// <summary>
    /// 串行执行
    /// <para>需等待上一个完成</para>
    /// </summary>
    [Description("串行执行")]
    Serial
}