namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据导出.
/// </summary>
[SuppressSniffer]
public class MetricDataIEExportInput
{
    /// <summary>
    /// 表名.
    /// </summary>
    [JsonProperty("tableName")]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 每页行数.
    /// </summary>
    [JsonProperty("pageSize")]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// 当前页码.
    /// </summary>
    [JsonProperty("currentPage")]
    public int CurrentPage { get; set; } = 1;

}