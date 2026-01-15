namespace Poxiao.Kpi.Application;

/// <summary>
/// 控制图.
/// </summary>
[SuppressSniffer]
public class AnalysisControlChartOutput
{
    /// <summary>
    /// 上管制限.
    /// </summary>
    [JsonProperty("uCL")]
    public double UCL { get; set; }

    /// <summary>
    /// 管制限.
    /// </summary>
    [JsonProperty("cL")]
    public double CL { get; set; }

    /// <summary>
    /// 下管制限.
    /// </summary>
    [JsonProperty("lCL")]
    public double LCL { get; set; }

    /// <summary>
    /// y轴数据.
    /// </summary>
    [JsonProperty("axis")]
    public List<double> Axis { get; set; } = new List<double>();
}

