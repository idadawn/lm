namespace Poxiao.Kpi.Core.Entitys;

/// <summary>
/// 指标分析任务.
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2024-1-9
/// </summary>
[SugarTable("metric_analysis_task", TableDescription = "指标分析任务")]
public class MetricAnalysisTaskEntity : CUEntityBase
{
    /// <summary>
    /// 指标Id.
    /// </summary>
    [SugarColumn(ColumnName = "metric_id", ColumnDescription = "指标Id")]
    public string MetricId { get; set; }

    /// <summary>
    /// 时间维度.
    /// </summary>
    [SugarColumn(ColumnName = "time_dimensions", ColumnDescription = "时间维度")]
    public string? TimeDimensions { get; set; }

    /// <summary>
    /// 维度.
    /// </summary>
    [SugarColumn(ColumnName = "dimensions", ColumnDescription = "维度")]
    public string Dimensions { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [SugarColumn(ColumnName = "filters", ColumnDescription = "筛选")]
    public string? Filters { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    [SugarColumn(ColumnName = "start_data", ColumnDescription = "开始时间")]
    public string StartData { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    [SugarColumn(ColumnName = "end_data", ColumnDescription = "结束时间")]
    public string EndData { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "status", ColumnDescription = "状态")]
    public AnalysisStatus Status { get; set; } = AnalysisStatus.InProgress;
}