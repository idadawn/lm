namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标仪表板接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-21.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricDash", Name = "MetricDash", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricDashController : IDynamicApiController
{
    private readonly IMetricDashService _metricDashService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricDashService"></param>
    public MetricDashController(IMetricDashService MetricDashService)
    {
        _metricDashService = MetricDashService;
    }

    /// <summary>
    /// 获取指标仪表板信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<MetricDashInfoOutput> GetInfo(string id)
    {
        var info = await _metricDashService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 新建指标仪表板.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricDashCrInput input)
    {
        var isOk = await _metricDashService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

}