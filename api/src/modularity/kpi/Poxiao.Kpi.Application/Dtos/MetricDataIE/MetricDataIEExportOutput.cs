namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据导出.
/// </summary>
[SuppressSniffer]
public class MetricDataIEExportOutput
{
    /// <summary>
    /// Dictionary 一行数据,
    /// string 字段名，object 字段数据.
    /// </summary>
    [JsonProperty("data")]
    public List<IDictionary<string, object?>> Data { get; set; } = new();

    /// <summary>
    /// 总页数.
    /// </summary>
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }
}