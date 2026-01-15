namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据筛选接口.
/// </summary>
public class MetricDataQryInput
{
    /// <summary>
    /// 指标Id.
    /// </summary>
    [JsonProperty("metricId")]
    [Required(ErrorMessage = "指标id不能为空")]
    public string MetricId { get; set; }

    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("dimensions")]
    public TableFieldOutput? Dimensions { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [JsonProperty("filters")]
    public List<MetricFilterDto>? Filters { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [JsonProperty("is_show_all")]
    public bool IsShowAll { get; set; } = false;

    /// <summary>
    /// 限制条数.
    /// </summary>
    [JsonProperty("limit")]
    public long Limit { get; set; } = 0;

    /// <summary>
    /// 升序 ASC 降序 DESC
    /// </summary>
    [JsonProperty("sortBy")]
    public DBSortByType SortBy { get; set; } = DBSortByType.ASC;

    /// <summary>
    /// 时间维度.
    /// </summary>
    [JsonProperty("time_dim")]
    public string? TimeDimension { get; set; }
}

/// <summary>
/// 多个指标数据筛选接口.
/// </summary>
public class MoreMetricDataQryInput
{
    /// <summary>
    /// 指标Id.
    /// </summary>
    [JsonProperty("metricId")]
    [Required(ErrorMessage = "指标id不能为空")]
    public List<string> MetricId { get; set; } = new List<string>();

    /// <summary>
    /// 维度.
    /// </summary>
    [JsonProperty("dimensions")]
    public TableFieldOutput? Dimensions { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [JsonProperty("filters")]
    public List<MetricFilterDto>? Filters { get; set; }

    /// <summary>
    /// 筛选.
    /// </summary>
    [JsonProperty("is_show_all")]
    public bool IsShowAll { get; set; } = false;

    /// <summary>
    /// 限制条数.
    /// </summary>
    [JsonProperty("limit")]
    public long Limit { get; set; } = 0;

    /// <summary>
    /// 升序 ASC 降序 DESC
    /// </summary>
    [JsonProperty("sortBy")]
    public DBSortByType SortBy { get; set; } = DBSortByType.ASC;
}
