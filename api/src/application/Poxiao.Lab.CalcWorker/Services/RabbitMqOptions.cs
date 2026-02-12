namespace Poxiao.Lab.CalcWorker.Services;

/// <summary>
/// RabbitMQ 配置选项
/// </summary>
public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    /// <summary>
    /// 计算任务队列名称
    /// </summary>
    public string TaskQueueName { get; set; } = "lab.calc.task";

    /// <summary>
    /// 判定任务队列名称
    /// </summary>
    public string JudgeQueueName { get; set; } = "lab.judge.task";

    /// <summary>
    /// 计算进度队列名称
    /// </summary>
    public string ProgressQueueName { get; set; } = "lab.calc.progress";

    /// <summary>
    /// 预取消息数量（建议 >= MaxConcurrency）
    /// </summary>
    public ushort PrefetchCount { get; set; } = 20;

    /// <summary>
    /// 最大并发消费数量（SemaphoreSlim 控制）
    /// </summary>
    public int MaxConcurrency { get; set; } = 20;

    /// <summary>
    /// 进度推送间隔（每 N 条完成推送一次进度到前端）
    /// </summary>
    public int ProgressInterval { get; set; } = 10;
}
