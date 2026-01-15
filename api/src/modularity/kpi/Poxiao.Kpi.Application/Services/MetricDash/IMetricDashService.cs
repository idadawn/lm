namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标仪表板接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public interface IMetricDashService
{
    /// <summary>
    /// 获取指标仪表板信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标仪表板信息.</returns>
    Task<MetricDashInfoOutput> GetAsync(string id);

    /// <summary>
    /// 新建指标仪表板.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricDashCrInput input);

}