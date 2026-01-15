namespace Poxiao.Kpi.Application;

/// <summary>
/// 新建指标仪表板.
/// </summary>
[SuppressSniffer]
public class MetricDashCrInput
{
    /// <summary>
    /// 思维图id.
    /// </summary>
    [JsonProperty("gotId")]
    public string? GotId { get; set; }

    /// <summary>
    /// 思维图类别.
    /// </summary>
    [JsonProperty("gotType")]
    public string? GotType { get; set; }

    /// <summary>
    /// 表单数据.
    /// </summary>
    [JsonProperty("formJson")]
    public string? FormJson { get; set; }

}