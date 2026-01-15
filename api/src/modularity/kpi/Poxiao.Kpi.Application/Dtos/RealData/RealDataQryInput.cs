namespace Poxiao.Kpi.Application;

/// <summary>
/// 实时数据查询.
/// </summary>
[SuppressSniffer]
public class RealDataQryInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 关键字.
    /// </summary>
    [JsonProperty("key")]
    public string key { get; set; }

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



/// <summary>
/// 实时数据查询.
/// </summary>
[SuppressSniffer]
public class RealDataAggQueryInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 关键字.
    /// </summary>
    [JsonProperty("key")]
    public string key { get; set; }

    /// <summary>
    /// 默认获取数据分钟数.
    /// </summary>
    public int Min { get; set; } = 60;

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