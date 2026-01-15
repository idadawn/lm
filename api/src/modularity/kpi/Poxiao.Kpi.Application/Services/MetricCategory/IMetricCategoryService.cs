namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分类接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-22.
/// </summary>
public interface IMetricCategoryService
{
    /// <summary>
    /// 获取指标分类信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标分类信息.</returns>
    Task<MetricCategoryInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取指标分类列表.
    /// </summary>
    /// <param name="input">查询条件.</param>
    /// <returns>指标分类列表.</returns>
    Task<List<MetricCategoryListOutput>> GetListAsync(MetricCategoryListQueryInput input);

    /// <summary>
    /// 新建指标分类.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricCategoryCrInput input);

    /// <summary>
    /// 更新指标分类.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricCategoryUpInput input);

    /// <summary>
    /// 删除指标分类.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<int> DeleteAsync(string id);

    /// <summary>
    /// 获取指标分类树或者下拉.
    /// </summary>
    /// <returns></returns>
    Task<List<MetricCategoryListOutput>> GetSelector();
}