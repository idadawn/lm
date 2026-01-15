namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标思维图接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-18.
/// </summary>
public interface IMetricGotService
{
    /// <summary>
    /// 获取指标思维图信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标思维图信息.</returns>
    Task<MetricGotInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取指标思维图列表
    /// </summary>
    /// <param name="type">类别.</param>
    /// <param name="input">查询条件.</param>
    /// <returns>指标思维图列表.</returns>
    Task<PagedResultDto<MetricGotListOutput>> GetListAsync(GotType type, MetricGotListQueryInput input);

    /// <summary>
    /// 新建指标思维图.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricGotCrInput input);

    /// <summary>
    /// 更新指标思维图.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricGotUpInput input);

    /// <summary>
    /// 删除指标思维图.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<int> DeleteAsync(string id);

    /// <summary>
    /// 获取某个标签下的所有思维图.
    /// </summary>
    /// <param name="tag">标签.</param>
    /// <returns></returns>
    Task<List<string>> GetGotIdByTag(string tag);
}