namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标聚合列表
/// </summary>
public class MetricAggInfoListOutput
{
    /// <summary>
    /// 聚合方式.
    /// </summary>
    [JsonProperty("aggType")]
    public string AggType { get; set; }

    /// <summary>
    /// 显示名称.
    /// </summary>
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 是否可用.
    /// </summary>
    [JsonProperty("isDisable")]
    public bool IsDisable { get; set; }
}