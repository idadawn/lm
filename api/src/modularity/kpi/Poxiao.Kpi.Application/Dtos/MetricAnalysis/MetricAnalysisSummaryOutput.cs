namespace Poxiao.Kpi.Application;


/// <summary>
/// 指标分析任务创建成功返回.
/// </summary>
public class MetricAnalysisSummaryOutput
{
    /// <summary>
    /// 任务Id.
    /// </summary>
    [JsonProperty("task_id")]
    public string TaskId { get; set; }

    /// <summary>
    /// 任务状态.
    /// </summary>
    [JsonProperty("task_status")]
    public AnalysisStatus TaskStatus { get; set; } = AnalysisStatus.InProgress;

    /// <summary>
    /// 分析内容.
    /// </summary>
    [JsonProperty("summary_content")]
    public string SummaryContent { get; set; }


    /// <summary>
    /// 信息.
    /// </summary>
    [JsonProperty("msg")]
    public string Msg { get; set; }
}

