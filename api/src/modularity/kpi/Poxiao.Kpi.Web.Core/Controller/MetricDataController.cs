namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标数据展示接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-21.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricData", Name = "metric_data", Order = 210)]
[Route("api/kpi/v1/[controller]")]
public class MetricDataController : IDynamicApiController
{
    private readonly IMetricDataService _metricDataService;

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="metricDataService"></param>
    public MetricDataController(IMetricDataService metricDataService)
    {
        _metricDataService = metricDataService;
    }

    /// <summary>
    /// 获取指标数据.
    /// </summary>
    /// <param name="metricId"></param>
    /// <return></return>
    [HttpGet("{metricId}")]
    public async Task<MetricDataOutput> GetDataAsync(string metricId)
    {
        return await _metricDataService.GetDataAsync(metricId);
    }

    /// <summary>
    /// 获取指标数据.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <return></return>
    [HttpPost("")]
    public async Task<MetricChartDataOutput> GetChartDataAsync(MetricDataQryInput input)
    {
        return await _metricDataService.GetChartDataAsync(input);
    }

    /// <summary>
    /// 获取指标数据.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <return></return>
    [HttpPost("more")]
    public async Task<MoreMetricChartDataOutput> GetMoreChartDataAsync(MoreMetricDataQryInput input)
    {
        return await _metricDataService.GetMoreChartDataAsync(input);
    }

}
