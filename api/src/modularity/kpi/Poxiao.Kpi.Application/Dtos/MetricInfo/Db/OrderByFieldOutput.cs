namespace Poxiao.Kpi.Application;

/// <summary>
/// 排序类型
/// </summary>
[SuppressSniffer]
public class OrderByFieldOutput : TableFieldOutput
{
    /// <summary>
    /// 升序 ASC 降序 DESC
    /// </summary>
    [JsonProperty("sortBy")]
    public DBSortByType SortBy { get; set; } = DBSortByType.ASC;
}
