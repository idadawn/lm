namespace Poxiao.TaskScheduler;

/// <summary>
/// 解析 Cron 表达式出错异常类
/// </summary>
/// <remarks>TaskScheduler 将于2022年12月31日移除，请使用 Scheduler 替代</remarks>
[Obsolete("The <TaskScheduler> will be removed on December 31, 2022. Please use the <Scheduler> instead.")]
[SuppressSniffer, Serializable]
public class CronFormatException : FormatException
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message"></param>
    public CronFormatException(string message) : base(message)
    {
    }

    /// <summary>
    /// 内部构造函数
    /// </summary>
    /// <param name="field"></param>
    /// <param name="message"></param>
    internal CronFormatException(CronField field, string message) : this($"{field}: {message}")
    {
    }
}