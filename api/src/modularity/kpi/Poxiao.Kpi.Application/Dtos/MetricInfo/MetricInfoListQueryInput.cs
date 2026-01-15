namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标定义查询信息.
/// </summary>
[SuppressSniffer]
public class MetricInfoListQueryInput : PageInputBase
{
    /// <summary>
    /// 指标类型.
    /// </summary>
    [JsonProperty("type")]
    public MetricType? Type { get; set; }

    /// <summary>
    /// 是否启用.
    /// </summary>
    [JsonProperty("isEnabled")]
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 标签.
    /// </summary>
    [JsonProperty("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 是否显示已删除的数据
    /// </summary>
    [JsonProperty("isShowDeleted")]
    public int? IsShowDeleted { get; set; }
}