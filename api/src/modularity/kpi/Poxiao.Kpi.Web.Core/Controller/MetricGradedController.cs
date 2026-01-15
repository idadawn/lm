namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标分级接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2024-1-5.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricGraded", Name = "metric-graded", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricGradedController : IDynamicApiController
{
    private readonly IMetricGradedService _metricGradedService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricGradedService">参数.</param>
    public MetricGradedController(IMetricGradedService MetricGradedService)
    {
        _metricGradedService = MetricGradedService;
    }

    /// <summary>
    /// 获取指标分级信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricGradedService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取指标分级列表.
    /// </summary>
    /// <param name="metricId">指标id.</param>
    /// <returns></returns>
    [HttpPost("list/{metricId}")]
    public async Task<List<MetricGradedListOutput>> GetList(string metricId)
    {
        var list = await _metricGradedService.GetListAsync(metricId);
        return list;
    }

    /// <summary>
    /// 新建指标分级.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricGradedCrInput input)
    {
        var isOk = await _metricGradedService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新指标分级.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricGradedUpInput input)
    {
        input.Id = id;
        var isOk = await _metricGradedService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除指标分级.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricGradedService.DeleteAsync(id);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}