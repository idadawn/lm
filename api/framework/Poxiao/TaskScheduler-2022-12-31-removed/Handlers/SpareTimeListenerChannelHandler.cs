using Poxiao.IPCChannel;

namespace Poxiao.TaskScheduler;

/// <summary>
/// 定时器监听管道处理程序
/// </summary>
internal sealed class SpareTimeListenerChannelHandler : ChannelHandler<SpareTimerExecuter>
{
    /// <summary>
    /// 触发程序
    /// </summary>
    /// <param name="executer"></param>
    /// <returns></returns>
    /// <remarks>TaskScheduler 将于2022年12月31日移除，请使用 Scheduler 替代</remarks>
    [Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]
    public async override Task InvokeAsync(SpareTimerExecuter executer)
    {
        var spareTimeListener = App.GetService<ISpareTimeListener>(App.RootServices);
        if (spareTimeListener == null) return;

        await spareTimeListener.OnListener(executer);
    }
}