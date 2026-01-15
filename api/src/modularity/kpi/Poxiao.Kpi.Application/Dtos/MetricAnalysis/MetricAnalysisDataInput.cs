namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分析数据输入.
/// </summary>
public class MetricAnalysisDataInput
{
    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("dimension")]
    public TableFieldOutput? Dimension { get; set; }

    /// <summary>
    /// 维度名称.
    /// </summary>
    [JsonProperty("dimension_name")]
    public string DimensionName { get; set; }

    /// <summary>
    /// 维度数据.
    /// </summary>
    [JsonProperty("data")]
    public List<ChartData> Data { get; set; } = new List<ChartData>();

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
