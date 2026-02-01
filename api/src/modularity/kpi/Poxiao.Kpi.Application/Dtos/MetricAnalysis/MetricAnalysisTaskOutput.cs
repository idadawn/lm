namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分析任务创建成功返回.
/// </summary>
public class MetricAnalysisTaskStatusOutput
{
    /// <summary>
    /// 任务id
    /// </summary>
    [JsonProperty("task_id")]
    public string TaskId { get; set; }

    /// <summary>
    /// 任务状态.
    /// </summary>
    [JsonProperty("task_status")]
    public AnalysisStatus TaskStatus { get; set; } = AnalysisStatus.InProgress;

}

/// <summary>
/// 指标分析任务创建成功返回.
/// </summary>
public class MetricAnalysisTaskOutput
{
    /// <summary>
    /// 任务id
    /// </summary>
    [JsonProperty("task_id")]
    public string TaskId { get; set; }

    /// <summary>
    /// 任务状态.
    /// </summary>
    [JsonProperty("task_status")]
    public AnalysisStatus TaskStatus { get; set; } = AnalysisStatus.InProgress;

    /// <summary>
    /// 开始时间.
    /// </summary>
    [JsonProperty("start_data")]
    public string StartData { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    [JsonProperty("end_data")]
    public string EndData { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    [JsonProperty("start")]
    public string Start { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    [JsonProperty("end")]
    public string End { get; set; }

    /// <summary>
    /// 趋势值.
    /// </summary>
    [JsonProperty("trend")]
    public TrendType Trend { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }

    /// <summary>
    /// 百分比.
    /// </summary>
    [JsonProperty("percentage")]
    public string Percentage { get; set; }
}

