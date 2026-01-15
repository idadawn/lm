namespace Poxiao.Kpi.Application;

/// <summary>
/// 正态分布及直方图轴数据信息.
/// </summary>
[SuppressSniffer]
public class AnalysisDataNormalListOutput
{
    /// <summary>
    /// 正态分布x轴.
    /// </summary>
    [JsonProperty("xAxis")]
    public List<double> XAxis { get; set; } = new List<double>();

    /// <summary>
    /// 正态分布y轴.
    /// </summary>
    [JsonProperty("yAxis")]
    public List<double> YAxis { get; set; } = new List<double>();

    /// <summary>
    /// 直方图x轴.
    /// </summary>
    [JsonProperty("xAxisHistogram")]
    public List<double> XAxisHistogram { get; set; } = new List<double>();

    /// <summary>
    /// 直方图y轴.
    /// </summary>
    [JsonProperty("yAxisHistogram")]
    public List<double> YAxisHistogram { get; set; } = new List<double>();
}