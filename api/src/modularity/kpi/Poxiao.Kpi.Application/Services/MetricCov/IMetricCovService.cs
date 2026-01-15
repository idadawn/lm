namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标价值链接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public interface IMetricCovService
{
    /// <summary>
    /// 获取指标价值链信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标价值链信息.</returns>
    Task<MetricCovInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取指标价值链列表
    /// </summary>
    /// <param name="gotId">思维图id.</param>
    /// <returns>指标价值链列表.</returns>
    Task<List<MetricCovListOutput>> GetListAsync(string gotId);

    /// <summary>
    /// 新建指标价值链.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<MetricCovInfoOutput> CreateAsync(MetricCovCrInput input);

    /// <summary>
    /// 更新指标价值链.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricCovUpInput input);

    /// <summary>
    /// 删除指标价值链.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// 获取所有指标价值链树.
    /// </summary>
    /// <returns></returns>
    Task<List<MetricCovSelectorOutput>> GetSelector();

    /// <summary>
    /// 获取指标价值链列表
    /// </summary>
    /// <param name="tag">标签.</param>
    /// <returns>指标价值链列表.</returns>
    Task<List<MetricCovListOutput>> GetKpiListAsync(string tag);
}