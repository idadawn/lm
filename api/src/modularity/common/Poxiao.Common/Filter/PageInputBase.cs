namespace Poxiao.Infrastructure.Filter;

/// <summary>
/// 通用分页输入参数.
/// </summary>
[SuppressSniffer]
public class PageInputBase : KeywordInput
{
    /// <summary>
    /// 查询条件.
    /// </summary>
    [JsonProperty("queryJson")]
    public virtual string QueryJson { get; set; } = string.Empty;

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
    /// 排序字段:sortField.
    /// </summary>
    [JsonProperty("sidx")]
    public virtual string Sidx { get; set; } = string.Empty;

    /// <summary>
    /// 排序类型:sortType.
    /// </summary>
    [JsonProperty("sort")]
    public virtual string Sort { get; set; } = "desc";

    /// <summary>
    /// 菜单ID.
    /// </summary>
    [JsonProperty("menuId")]
    public virtual string? MenuId { get; set; }
}