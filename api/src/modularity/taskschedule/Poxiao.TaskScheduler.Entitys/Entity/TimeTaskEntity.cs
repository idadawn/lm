using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.TaskScheduler.Entitys;

/// <summary>
/// 定时任务
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01 .
/// </summary>
[SugarTable("BASE_TIMETASK")]
public class TimeTaskEntity : CLDEntityBase
{
    /// <summary>
    /// 任务编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 任务名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 执行类型
    /// 1:数据接口 3:本地方法.
    /// </summary>
    [SugarColumn(ColumnName = "F_EXECUTETYPE")]
    public string ExecuteType { get; set; }

    /// <summary>
    /// 执行内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_EXECUTECONTENT")]
    public string ExecuteContent { get; set; }

    /// <summary>
    /// 执行周期.
    /// </summary>
    [SugarColumn(ColumnName = "F_EXECUTECYCLEJSON")]
    public string ExecuteCycleJson { get; set; }

    /// <summary>
    /// 最后运行时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LASTRUNTIME")]
    public DateTime? LastRunTime { get; set; }

    /// <summary>
    /// 下次运行时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_NEXTRUNTIME")]
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    /// 运行次数.
    /// </summary>
    [SugarColumn(ColumnName = "F_RUNCOUNT")]
    public int? RunCount { get; set; } = 0;

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}
