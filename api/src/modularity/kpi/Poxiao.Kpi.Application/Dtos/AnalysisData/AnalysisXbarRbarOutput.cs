namespace Poxiao.Kpi.Application;

/// <summary>
/// 均值-极差控制图数据信息.
/// </summary>
[SuppressSniffer]
public class AnalysisXbarRbarOutput
{
    /// <summary>
    /// 均值图.
    /// </summary>
    [JsonProperty("average")]
    public AnalysisControlChartOutput Average { get; set; } = new AnalysisControlChartOutput();

    /// <summary>
    /// 极差图.
    /// </summary>
    [JsonProperty("range")]
    public AnalysisControlChartOutput Range { get; set; } = new AnalysisControlChartOutput();
}

