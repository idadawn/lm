namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据导入导出，添加指标数据.
/// </summary>
[SuppressSniffer]
public class MetricDataIEInsertTableInput
{
    /// <summary>
    /// 表名.
    /// </summary>
    [JsonProperty("tableName")]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 要插入的数据,
    /// Dictionary 一行数据,
    /// string 字段名，object 字段数据.
    /// </summary>
    [JsonProperty("data")]
    public List<Dictionary<string, object>> Data { get; set; }
}
