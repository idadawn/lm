namespace Poxiao.TaskQueue;

/// <summary>
/// 任务队列配置选项构建器
/// </summary>
[SuppressSniffer]
public sealed class TaskQueueOptionsBuilder
{
    /// <summary>
    /// 默认内置任务队列内存通道容量
    /// </summary>
    /// <remarks>超过 n 条待处理消息，第 n+1 条将进入等待，默认为 3000</remarks>
    public int ChannelCapacity { get; set; } = 3000;

    /// <summary>
    /// 未察觉任务异常事件处理程序
    /// </summary>
    public EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskExceptionHandler { get; set; }

    /// <summary>
    /// 构建任务配置选项
    /// </summary>
    internal void Build()
    {
    }
}