namespace Poxiao.Kpi.Application;

/// <summary>
/// 公共维度接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-6.
/// </summary>
public interface IMetricDimensionService
{
    /// <summary>
    /// 获取公共维度信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>公共维度信息.</returns>
    Task<MetricDimensionInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取公共维度列表
    /// </summary>
    /// <param name="input">查询条件.</param>
    /// <returns>公共维度列表.</returns>
    Task<PagedResultDto<MetricDimensionListOutput>> GetListAsync(MetricDimensionListQueryInput input);

    /// <summary>
    /// 新建公共维度.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricDimensionCrInput input);

    /// <summary>
    /// 更新公共维度.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricDimensionUpInput input);

    /// <summary>
    /// 删除公共维度.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// 获取所有公共维度选项.
    /// </summary>
    /// <returns></returns>
    Task<List<MetricDimOptionsOutput>> GetOptionsAsync();

    /// <summary>
    /// 根据维度id查询维度数据.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ModelDataListOutput> GetDimensionDataAsync(string id);
}