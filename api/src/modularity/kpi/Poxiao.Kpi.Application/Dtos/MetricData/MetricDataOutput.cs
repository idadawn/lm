namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据.
/// </summary>
public class MetricDataOutput
{
    /// <summary>
    /// 指标信息.
    /// </summary>
    [JsonProperty("metricInfo")]
    public MetricBasicInfo MetricInfo { get; set; }

    /// <summary>
    /// 数据.
    /// </summary>
    [JsonProperty("data")]
    public ModelDataOutput Data { get; set; }

}

/// <summary>
/// 指标基础信息
/// </summary>
public class MetricBasicInfo
{
    /// <summary>
    /// 类别.
    /// </summary>
    [JsonProperty("type")]
    public MetricType Type { get; set; }

    /// <summary>
    /// 指标名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 指标编码.
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; }
}