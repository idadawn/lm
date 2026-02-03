namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据服务.
/// </summary>
public interface IMetricDataService
{
    /// <summary>
    /// 根据指标id,获取指标数据.
    /// </summary>
    /// <param name="metricId">指标id.</param>
    /// <returns></returns>
    Task<MetricDataOutput> GetDataAsync(string metricId, DisplayOption? displayOption = null);

    /// <summary>
    /// 获取指标图标数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<MetricChartDataOutput> GetChartDataAsync(MetricDataQryInput input);

    /// <summary>
    /// 获取指标图标数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<MoreMetricChartDataOutput> GetMoreChartDataAsync(MoreMetricDataQryInput input);
}
