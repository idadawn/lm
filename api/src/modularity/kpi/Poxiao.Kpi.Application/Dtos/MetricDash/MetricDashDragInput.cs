namespace Poxiao.Kpi.Application;


/// <summary>
/// 拖拽指标.
/// </summary>
[SuppressSniffer]
public class MetricDashDragInput
{
    /// <summary>
    /// 已有指标id.
    /// </summary>
    public List<string> Metrics { get; set; }

    /// <summary>
    /// 当前指标.
    /// </summary>
    public string CurrentMetricId { get; set; }
}