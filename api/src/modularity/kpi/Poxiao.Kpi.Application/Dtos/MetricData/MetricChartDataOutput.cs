namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标图表数据.
/// </summary>
public class MetricChartDataOutput
{
    /// <summary>
    /// 指标信息.
    /// </summary>
    [JsonProperty("metricInfo")]
    public MetricBasicInfo MetricInfo { get; set; }

    /// <summary>
    /// 图表数据.
    /// </summary>
    [JsonProperty("data")]
    public ModelChartDataOutput Data { get; set; }

    /// <summary>
    /// 指标分级.
    /// </summary>
    [JsonProperty("metric_grade")]
    public List<MetricInfoGradeExtInput> MetricInfoGrade { get; set; }
}

/// <summary>
/// 指标图表数据.
/// </summary>
public class MoreMetricChartDataOutput
{
    /// <summary>
    /// 指标信息.
    /// </summary>
    [JsonProperty("metricInfo")]
    public List<MetricBasicInfo> MetricInfo { get; set; } = new List<MetricBasicInfo>();

    /// <summary>
    /// 图表数据.
    /// </summary>
    [JsonProperty("data")]
    public MoreModelChartDataOutput Data { get; set; } = new MoreModelChartDataOutput();

    /// <summary>
    /// 限制条数.
    /// </summary>
    [JsonProperty("limit")]
    public long Limit { get; set; }

    /// <summary>
    /// 总耗时
    /// </summary>
    [JsonProperty("total_time")]
    public long TotalTime { get; set; }

    /// <summary>
    /// 维度信息.
    /// </summary>
    [JsonProperty("dimensions")]
    public List<string> Dimensions { get; set; } = new List<string>();

    /// <summary>
    /// 显示名称.
    /// </summary>
    [JsonProperty("display_names")]
    public List<string> DisplayNames { get; set; } = new List<string>();

    /// <summary>
    /// 指标名称.
    /// </summary>
    [JsonProperty("metric_names")]
    public List<string> MetricNames { get; set; } = new List<string>();
}