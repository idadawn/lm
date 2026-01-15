namespace Poxiao.Kpi.Application;

/// <summary>
/// 模型数据信息
/// </summary>
[SuppressSniffer]
public class ModelDataListOutput
{
    /// <summary>
    /// 指标筛选方式
    /// </summary>
    [JsonProperty("filterModel")]
    public List<string> FilterModel { get; set; } = new List<string>();

    /// <summary>
    /// 数据
    /// </summary>
    [JsonProperty("data")]
    public List<string> Data { get; set; } = new List<string>();

    /// <summary>
    /// 执行的sql
    /// </summary>
    [JsonProperty("executed_sql")]
    public string ExecutedSql { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    [JsonProperty("metas")]
    public List<TableFieldOutput> Metas { get; set; } = new List<TableFieldOutput>();

    /// <summary>
    /// 总耗时
    /// </summary>
    [JsonProperty("total_time")]
    public long TotalTime { get; set; }
}

/// <summary>
/// 模型数据信息
/// </summary>
[SuppressSniffer]
public class ModelDataOutput
{
    /// <summary>
    /// 数据
    /// </summary>
    [JsonProperty("data")]
    public string Data { get; set; }

    /// <summary>
    /// 执行的sql
    /// </summary>
    [JsonProperty("executed_sql")]
    public string ExecutedSql { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    [JsonProperty("metas")]
    public List<TableFieldOutput> Metas { get; set; } = new List<TableFieldOutput>();

    /// <summary>
    /// 总耗时
    /// </summary>
    [JsonProperty("total_time")]
    public long TotalTime { get; set; }
}

/// <summary>
/// 模型图表数据信息
/// </summary>
[SuppressSniffer]
public class ModelChartDataOutput
{
    /// <summary>
    /// 限制条数.
    /// </summary>
    [JsonProperty("limit")]
    public long Limit { get; set; }

    /// <summary>
    /// 图表数据.
    /// </summary>
    [JsonProperty("data")]
    public List<List<object?>> Data { get; set; }

    /// <summary>
    /// 图表数据.
    /// </summary>
    [JsonIgnore]
    public List<ChartData> List { get; set; } = new List<ChartData>();

    /// <summary>
    /// 执行的sql
    /// </summary>
    [JsonProperty("executed_sql")]
    public string ExecutedSql { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    [JsonProperty("metas")]
    public List<TableFieldOutput> Metas { get; set; } = new List<TableFieldOutput>();

    /// <summary>
    /// 总耗时
    /// </summary>
    [JsonProperty("total_time")]
    public long TotalTime { get; set; }
}

/// <summary>
/// 图表数据.
/// </summary>
[SuppressSniffer]
public class ChartData
{
    /// <summary>
    /// 维度信息.
    /// </summary>
    [JsonProperty("dimension")]
    public string Dimension { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    [JsonProperty("value")]
    public string? Value { get; set; }
}

/// <summary>
/// 模型图表数据信息
/// </summary>
[SuppressSniffer]
public class MoreModelChartDataOutput
{
    /// <summary>
    /// 图表数据.
    /// </summary>
    [JsonProperty("data")]
    public List<List<object>> Data { get; set; }

    /// <summary>
    /// 执行的sql
    /// </summary>
    [JsonProperty("executed_sql")]
    public string ExecutedSql { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    [JsonProperty("metas")]
    public List<TableFieldOutput> Metas { get; set; }

}
