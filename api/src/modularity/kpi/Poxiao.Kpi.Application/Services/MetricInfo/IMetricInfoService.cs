namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标定义接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-12.
/// </summary>
public interface IMetricInfoService
{
    /// <summary>
    /// 获取指标定义信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns>指标定义信息.</returns>
    Task<MetricInfoInfoOutput> GetAsync(string id);

    /// <summary>
    /// 获取指标定义列表
    /// </summary>
    /// <param name="input">查询条件.</param>
    /// <returns>指标定义列表.</returns>
    Task<PagedResultDto<MetricInfoListOutput>> GetListAsync(MetricInfoListQueryInput input);

    /// <summary>
    /// 新建指标定义.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CreateAsync(MetricInfoCrInput input);

    /// <summary>
    /// 更新指标定义.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricInfoUpInput input);

    /// <summary>
    /// 删除指标定义.
    /// </summary>
    /// <param name="ids">主键.</param>
    /// <returns>成功数量.</returns>
    Task<int> DeleteAsync(List<string> ids);

    /// <summary>
    /// 获取聚合方式列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    Task<List<MetricAggInfoListOutput>> GetAggListAsync(TableFieldOutput input);

    /// <summary>
    /// 指标在线下线.
    /// true 上线 false 下线.
    /// </summary>
    /// <param name="ids">主键.</param>
    /// <param name="isEnabled">true 上线 false 下线.</param>
    /// <returns></returns>
    Task<int> SetEnableAsync(List<string> ids, bool isEnabled);

    /// <summary>
    /// 获取所有指标信息.
    /// </summary>
    /// <param name="isDerive">是否派生指标.</param>
    /// <returns></returns>
    Task<List<MetricInfoAllOutput>> GetAllAsync(bool isDerive);

    /// <summary>
    /// 检查当前指标名称.
    /// </summary>
    /// <param name="name">指标名称.</param>
    /// <returns></returns>
    Task<bool> CheckNameAsync(string name);

    /// <summary>
    /// 检查当前指标名称.
    /// </summary>
    /// <param name="code">指标名称.</param>
    /// <returns></returns>
    Task<bool> CheckCodeAsync(string code);

    /// <summary>
    /// 获取所有维度交集信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    Task<MetricInfoDimensionsOutput> GetDimensionsAsync(MetricInfoDimQryCrInput input);

    /// <summary>
    /// 获取所有维度并集信息.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<MetricInfoDimensionsOutput> GetDimsUnionAsync(MetricInfoDimQryCrInput input);

    /// <summary>
    /// 获取指标筛选数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    Task<ModelDataListOutput> GetFilterMetricDataAsync(ModelDataListQueryInput input);

    /// <summary>
    /// 拷贝指标.
    /// </summary>
    /// <param name="id">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> CopyAsync(string id);

    /// <summary>
    /// 根据指标id获取指标信息.
    /// </summary>
    /// <param name="metricId">指标id.</param>
    /// <returns></returns>
    Task<List<MetricInfoListOutput>> GetListByIdAsync(List<string> metricId);

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<List<TableFieldOutput>> GetRtSeriesListAsync(string name);

    /// <summary>
    /// 获取所有指标信息.
    /// </summary>
    /// <returns></returns>
    Task<List<MetricInfoListForChatDto>> GetAll4ChatAsync();

    /// <summary>
    /// 根据指标名称获取指标信息.
    /// </summary>
    /// <param name="name">指标名称.</param>
    /// <returns></returns>
    Task<MetricInfoInfoOutput> GetByNameAsync(string name);

    /// <summary>
    /// 获取指标图表数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    Task<ModelChartDataOutput> GetMetricChartDataAsync(ModelDataAggQueryInput input);
}
