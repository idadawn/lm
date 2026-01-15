namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标价值链规则接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public interface IMetricCovRuleService
{
    /// <summary>
    /// 获取指标价值链规则信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标价值链规则信息.</returns>
    Task<MetricCovRuleInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取指标价值链规则列表
    /// </summary>
    /// <param name="covId">节点id.</param>
    /// <returns>指标价值链规则列表.</returns>
    Task<List<MetricCovRuleListOutput>> GetListAsync(string covId);

    /// <summary>
    /// 新建指标价值链规则.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricCovRuleCrInput input);

    /// <summary>
    /// 更新指标价值链规则.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricCovRuleUpInput input);

    /// <summary>
    /// 删除指标价值链规则.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<bool> DeleteAsync(string id);
}