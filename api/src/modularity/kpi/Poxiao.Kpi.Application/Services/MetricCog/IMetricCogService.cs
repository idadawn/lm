namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标图链接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public interface IMetricCogService
{
    /// <summary>
    /// 获取指标图链信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标图链信息.</returns>
    Task<MetricCogInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取指标图链列表
    /// </summary>
    /// <param name="input">查询条件.</param>
    /// <returns>指标图链列表.</returns>
    Task<dynamic> GetListAsync(MetricCogListQueryInput input);

    /// <summary>
    /// 新建指标图链.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricCogCrInput input);

    /// <summary>
    /// 更新指标图链.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricCogUpInput input);

    /// <summary>
    /// 删除指标图链.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<int> DeleteAsync(string id);
}