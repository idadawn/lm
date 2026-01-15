namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标价值链接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-20.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricCov", Name = "MetricCov", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricCovController : IDynamicApiController
{
    private readonly IMetricCovService _metricCovService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricCovService"></param>
    public MetricCovController(IMetricCovService MetricCovService)
    {
        _metricCovService = MetricCovService;
    }

    /// <summary>
    /// 获取指标价值链信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricCovService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取指标价值链列表.
    /// </summary>
    /// <param name="gotId">思维图id.</param>
    /// <returns></returns>
    [HttpGet("list/{gotId}")]
    public async Task<List<MetricCovListOutput>> GetList(string gotId)
    {
        var list = await _metricCovService.GetListAsync(gotId);
        return list;
    }

    /// <summary>
    /// 新建指标价值链.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<MetricCovInfoOutput> CreateAsync([FromBody] MetricCovCrInput input)
    {
        var info = await _metricCovService.CreateAsync(input);
        if (info == null) throw Oops.Oh(ErrorCode.COM1000);
        return info;
    }

    /// <summary>
    /// 更新指标价值链.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricCovUpInput input)
    {
        input.Id = id;
        var isOk = await _metricCovService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除指标价值链.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricCovService.DeleteAsync(id);
        if (!isOk) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 获取所有指标价值链树.
    /// </summary>
    /// <returns></returns>
    [HttpGet("selector")]
    public async Task<List<MetricCovSelectorOutput>> GetSelector()
    {
        var list = await _metricCovService.GetSelector();
        return list;
    }

    /// <summary>
    /// 获取指标价值链列表.
    /// </summary>
    /// <param name="gotId">思维图id.</param>
    /// <returns></returns>
    [HttpGet("kpi/{tag}")]
    public async Task<List<MetricCovListOutput>> GetKpiListAsync(string tag)
    {
        var list = await _metricCovService.GetKpiListAsync(tag);
        return list;
    }
}