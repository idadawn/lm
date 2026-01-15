namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标分析任务.
/// </summary>
[SuppressSniffer]
public class MetricAnalysisTaskUpInput : MetricAnalysisTaskCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}