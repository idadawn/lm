namespace Poxiao.TaskScheduler;

/// <summary>
/// 定时器执行状态器
/// </summary>
/// <remarks>TaskScheduler 将于2022年12月31日移除，请使用 Scheduler 替代</remarks>
[Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]
[SuppressSniffer]
public sealed class SpareTimerExecuter
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="status"></param>
    public SpareTimerExecuter(SpareTimer timer, int status)
    {
        Timer = timer;
        Status = status;
    }

    /// <summary>
    /// 定时器
    /// </summary>
    public SpareTimer Timer { get; internal set; }

    /// <summary>
    /// 状态
    /// </summary>
    /// <remarks>
    /// <para>0：任务开始</para>
    /// <para>1：执行之前</para>
    /// <para>2：执行成功</para>
    /// <para>3：执行失败</para>
    /// <para>-1：任务停止</para>
    /// <para>-2：任务取消</para>
    /// </remarks>
    public int Status { get; internal set; }
}