namespace Poxiao.Kpi.Application;

/// <summary>
/// 定义一个管理度量标签的服务接口.
/// </summary>
public interface IMetricTagsService
{
    /// <summary>
    /// 获取标签信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>标签信息.</returns>
    Task<MetricTagsInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取标签列表
    /// </summary>
    /// <param name="input">查询条件.</param>
    /// <returns>标签列表.</returns>
    Task<dynamic> GetListAsync(MetricTagsListQueryInput input);

    /// <summary>
    /// 新建标签.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricTagCrInput input);

    /// <summary>
    /// 更新标签.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricTagUpInput input);

    /// <summary>
    /// 删除标签.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>成功数量.</returns>
    Task<int> DeleteAsync(string id);

    /// <summary>
    /// 获取下拉数据.
    /// </summary>
    /// <returns></returns>
    Task<List<MetricTagsListOutput>> GetSelector();
}