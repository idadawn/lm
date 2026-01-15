using Poxiao.TaskScheduler.Entitys;

namespace Poxiao.TaskScheduler.Interfaces.TaskScheduler;

/// <summary>
/// 定时任务
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
public interface ITimeTaskService
{
    /// <summary>
    /// 根据类型执行任务.
    /// </summary>
    /// <param name="entity">任务实体.</param>
    /// <returns></returns>
    Task<string> PerformJob(TimeTaskEntity entity);
}