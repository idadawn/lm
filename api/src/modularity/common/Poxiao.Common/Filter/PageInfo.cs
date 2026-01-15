using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Poxiao.Infrastructure.Filter;


/// <summary>
/// 分页结果.
/// </summary>
[SuppressSniffer]
public class PagedResultDto<T>
{
    /// <summary>
    /// 分页实体.
    /// </summary>
    [JsonProperty("pagination")]
    public PageInfo Pagination { get; set; }

    /// <summary>
    /// 数据.
    /// </summary>
    [JsonProperty("list")]
    public List<T> List { get; set; }

    /// <summary>
    /// 替换sqlsugar分页.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static PagedResultDto<T> SqlSugarPageResult(SqlSugarPagedList<T> page)
    {
        return new PagedResultDto<T>
        {
            Pagination = page.pagination.Adapt<PageInfo>(),
            List = page.list.Adapt<List<T>>()
        };
    }
}

/// <summary>
/// 分页结果.
/// </summary>
[SuppressSniffer]
public class PageResult<T>
{
    /// <summary>
    /// 分页实体.
    /// </summary>
    public PageInfo pagination { get; set; }

    /// <summary>
    /// 数据.
    /// </summary>
    public List<T> list { get; set; }

    /// <summary>
    /// 替换sqlsugar分页.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static dynamic SqlSugarPageResult(SqlSugarPagedList<T> page)
    {
        return new {
            pagination = page.pagination.Adapt<PageInfo>(),
            list = page.list
        };
    }
}

/// <summary>
/// 分页结果.
/// </summary>
[SuppressSniffer]
public class PageInfo
{
    /// <summary>
    /// 页码.
    /// </summary>
    public int currentPage { get; set; }

    /// <summary>
    /// 页容量.
    /// </summary>
    public int pageSize { get; set; }

    /// <summary>
    /// 总条数.
    /// </summary>
    public int total { get; set; }
}


