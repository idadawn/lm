using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.TaskScheduler.Entitys;

/// <summary>
/// 定时任务日志
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
[SugarTable("BASE_TIMETASKLOG")]
public class TimeTaskLogEntity : OEntityBase<string>
{
    /// <summary>
    /// 定时任务主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_TASKID")]
    public string TaskId { get; set; }

    /// <summary>
    /// 执行时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_RUNTIME")]
    public DateTime? RunTime { get; set; }

    /// <summary>
    /// 执行结果.
    /// </summary>
    [SugarColumn(ColumnName = "F_RUNRESULT")]
    public int? RunResult { get; set; }

    /// <summary>
    /// 执行说明.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }
}
