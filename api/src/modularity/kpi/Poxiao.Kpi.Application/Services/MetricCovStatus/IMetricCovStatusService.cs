namespace Poxiao.Kpi.Application;

/// <summary>
/// 价值链状态接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-6.
/// </summary>
public interface IMetricCovStatusService
{
    /// <summary>
    /// 获取价值链状态信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>价值链状态信息.</returns>
    Task<MetricCovStatusInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取价值链状态列表
    /// </summary>
    /// <param name="input">查询条件.</param>
    /// <returns>价值链状态列表.</returns>
    Task<PagedResultDto<MetricCovStatusListOutput>> GetListAsync(MetricCovStatusListQueryInput input);

    /// <summary>
    /// 新建价值链状态.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricCovStatusCrInput input);

    /// <summary>
    /// 更新价值链状态.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricCovStatusUpInput input);

    /// <summary>
    /// 删除价值链状态.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// 获取所有状态信息.
    /// </summary>
    /// <returns></returns>
    Task<List<MetricCovStatusOptionOutput>> GetOptionsAsync();
}