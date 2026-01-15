namespace Poxiao.Kpi.Application;

/// <summary>
/// 复合指标定义服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-12.
/// </summary>
public interface IMetricInfo4CompositeService
{
    /// <summary>
    /// 检查公式.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<bool> FormulaCheckAsync(FormulaInput input);

    /// <summary>
    /// 获取指标定义信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标定义信息.</returns>
    Task<MetricInfo4CompositeOutput> GetAsync(string id);

    /// <summary>
    /// 新建指标定义.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricInfo4CompositeCrInput input);

    /// <summary>
    /// 更新指标定义.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricInfo4CompositeUpInput input);

}