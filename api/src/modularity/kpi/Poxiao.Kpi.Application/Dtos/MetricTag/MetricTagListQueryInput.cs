namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标标签查询信息.
/// </summary>
[SuppressSniffer]
public class MetricTagsListQueryInput : PageInputBase
{
    /// <summary>
    /// 当前页码:pageIndex.
    /// </summary>
    [JsonProperty("currentPage")]
    public virtual int CurrentPage { get; set; } = 1;

    /// <summary>
    /// 每页行数.
    /// </summary>
    [JsonProperty("pageSize")]
    public virtual int PageSize { get; set; } = 50;

    /// <summary>
    /// 标签名称.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 是否显示已删除的数据
    /// </summary>
    [JsonProperty("isShowDeleted")]
    public int? IsShowDeleted { get; set; } = 0;
}