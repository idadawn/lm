namespace Poxiao.Kpi.Application;

/// <summary>
/// 获取模型查询信息.
/// </summary>
[SuppressSniffer]
public class ModelDataQueryInput
{
    /// <summary>
    /// 数据源id.
    /// </summary>
    [JsonProperty("linkId")]
    public string LinkId { get; set; }

    /// <summary>
    /// Schema名称.
    /// </summary>
    [JsonProperty("schemaName")]
    public string SchemaName { get; set; }

    /// <summary>
    /// 列字段集合.
    /// </summary>
    [JsonProperty("columnField")]
    public TableFieldOutput ColumnField { get; set; }

    /// <summary>
    /// 排序集合 字段:ASC .
    /// </summary>
    [JsonProperty("orderByField")]
    public OrderByFieldOutput OrderByField { get; set; }

}

/// <summary>
/// 获取模型查询信息.
/// </summary>
[SuppressSniffer]
public class ModelDataListQueryInput
{
    /// <summary>
    /// 数据源id.
    /// </summary>
    [JsonProperty("metrics")]
    public List<string> Metrics { get; set; }
    /// <summary>
    /// 列字段集合.
    /// </summary>
    [JsonProperty("columnField")]
    public TableFieldOutput ColumnField { get; set; }

    /// <summary>
    /// 排序集合 字段:ASC .
    /// </summary>
    [JsonProperty("orderByField")]
    public OrderByFieldOutput OrderByField { get; set; }

}

/// <summary>
/// 获取分组查询信息.
/// </summary>
public class ModelDataAggQueryInput
{
    /// <summary>
    /// 指标Id.
    /// </summary>
    [JsonProperty("metricId")]
    public string MetricId { get; set; }

    /// <summary>
    /// 任务Id.
    /// </summary>
    [JsonProperty("taskId")]
    public string TaskId { get; set; }

    /// <summary>
    /// 数据源id.
    /// </summary>
    [JsonProperty("linkId")]
    public string LinkId { get; set; }

    /// <summary>
    /// Schema名称.
    /// </summary>
    [JsonProperty("schemaName")]
    public string SchemaName { get; set; }

    /// <summary>
    /// 列字段集合.
    /// </summary>
    [JsonProperty("columnField")]
    public TableFieldOutput ColumnField { get; set; }

    /// <summary>
    /// 指标列聚合方式.
    /// </summary>
    [JsonProperty("aggType")]
    public DBAggType AggType { get; set; }

    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("dimensions")]
    public TableFieldOutput? Dimensions { get; set; }

    /// <summary>
    /// 时间粒度.
    /// </summary>
    [JsonProperty("granularity")]
    public GranularityType? Granularity { get; set; }

    /// <summary>
    /// 展示方式.
    /// </summary>
    [JsonProperty("displayOption")]
    public DisplayOption? DisplayOption { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [JsonProperty("filters")]
    public List<MetricFilterDto>? Filters { get; set; }
   
    /// <summary>
    /// 限制条数.
    /// </summary>
    [JsonProperty("limit")]
    public long Limit { get; set; }

    /// <summary>
    /// 升序 ASC 降序 DESC
    /// </summary>
    [JsonProperty("sortBy")]
    public DBSortByType SortBy { get; set; } = DBSortByType.ASC;
}
