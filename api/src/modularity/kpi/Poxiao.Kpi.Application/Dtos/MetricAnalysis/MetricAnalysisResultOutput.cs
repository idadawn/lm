namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分析结果返回.
/// </summary>
public class MetricAnalysisResultOutput
{
    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("start_time")]
    public long StartTime { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("task_id")]
    public string TaskId { get; set; }

    [JsonProperty("base_period")]
    public string BasePeriod { get; set; }

    [JsonProperty("base_period_value")]
    public int BasePeriodValue { get; set; }

    [JsonProperty("compared_period")]
    public string ComparedPeriod { get; set; }

    [JsonProperty("compared_period_value")]
    public int ComparedPeriodValue { get; set; }

    [JsonProperty("analysis_result")]
    public List<MetricAnalysisResult> AnalysisResult { get; set; }

    [JsonProperty("msg")]
    public string Msg { get; set; }


}

/// <summary>
/// 指标分析结果.
/// </summary>
public class MetricAnalysisResult
{
    [JsonProperty("attribution_list")]
    public List<AttributionList> AttributionList { get; set; }

    [JsonProperty("coefficient")]
    public int Coefficient { get; set; }

    [JsonProperty("dimension")]
    public string Dimension { get; set; }
}

/// <summary>
/// 趋势分析结果.
/// </summary>
public class AttributionList
{

    [JsonProperty("attribution_value")]
    public double AttributionValue { get; set; }

    [JsonProperty("base_period_value")]
    public int BasePeriodValue { get; set; }

    [JsonProperty("compared_period_value")]
    public int ComparedPeriodValue { get; set; }

    [JsonProperty("dimension_value")]
    public string DimensionValue { get; set; }
}