namespace Poxiao.Kpi.Application;

/// <summary>
/// 公共维度选项.
/// </summary>
public class MetricDimOptionsOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 维度名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}

