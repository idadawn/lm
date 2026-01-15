namespace Poxiao.Kpi.Application;

/// <summary>
/// 更新指标分类.
/// </summary>
[SuppressSniffer]
public class MetricCategoryUpInput : MetricCategoryCrInput
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}