namespace Poxiao.Kpi.Application;

/// <summary>
/// 维度.
/// </summary>
public class MetricInfoDimensionsOutput
{
    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("dimensions")]
    public List<TableFieldOutput>? Dimensions { get; set; }
}