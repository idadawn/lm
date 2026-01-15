namespace Poxiao.TaskScheduler;

/// <summary>
/// 定时器监听接口（注册为单例）
/// </summary>
/// <remarks>TaskScheduler 将于2022年12月31日移除，请使用 Scheduler 替代</remarks>
[Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]
public interface ISpareTimeListener
{
    /// <summary>
    /// 监听器
    /// </summary>
    /// <param name="executer"></param>
    /// <remarks>TaskScheduler 将于2022年12月31日移除，请使用 Scheduler 替代</remarks>
    [Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]
    Task OnListener(SpareTimerExecuter executer);
}