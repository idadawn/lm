namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据分析接口.
/// </summary>
public class MetricAnalysisTaskCrInput
{
    /// <summary>
    /// 指标Id.
    /// </summary>
    [JsonProperty("metric_id")]
    [Required(ErrorMessage = "指标id不能为空")]
    public string MetricId { get; set; }

    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("time_dimensions")]
    public MetricTimeDimensionDto? TimeDimensions { get; set; }

    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("dimensions")]
    public List<TableFieldOutput>? Dimensions { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [JsonProperty("filters")]
    public List<MetricFilterDto>? Filters { get; set; }

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
}
