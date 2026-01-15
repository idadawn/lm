namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据.
/// </summary>
public class MetricDataForChatDto
{
    /// <summary>
    /// 指标值
    /// </summary>
    [JsonProperty("value")]
    public string value { get; set; }

    /// <summary>
    /// 线性图表数据.
    /// 这个数据作为调用$line_chart$线性图表所需要的数据参数.
    /// </summary>
    [JsonProperty("data")]
    public string Data { get; set; }

    /// <summary>
    /// 线性图表x轴.
    /// 这个数据作为调用$line_chart$线性图表所需要的x轴参数.
    /// </summary>
    [JsonProperty("x_axis")]
    public string XAxis { get; set; }

}