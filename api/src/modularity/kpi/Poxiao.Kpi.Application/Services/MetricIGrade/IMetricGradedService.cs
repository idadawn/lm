namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分级接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2024-1-5.
/// </summary>
public interface IMetricGradedService
{
    /// <summary>
    /// 获取指标分级信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标分级信息.</returns>
    Task<MetricGradedInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取指标分级列表
    /// </summary>
    /// <param name="metricId">指标id.</param>
    /// <returns>指标分级列表.</returns>
    Task<List<MetricGradedListOutput>> GetListAsync(string metricId);

    /// <summary>
    /// 新建指标分级.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricGradedCrInput input);

    /// <summary>
    /// 更新指标分级.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricGradedUpInput input);

    /// <summary>
    /// 删除指标分级.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<int> DeleteAsync(string id);

    /// <summary>
    /// 根据指标Id获取指标分级信息.
    /// </summary>
    /// <param name="metricId">指标id.</param>
    /// <returns></returns>
    Task<List<MetricInfoGradeExtInput>> GetGradeExtInfoAsync(string metricId);
}