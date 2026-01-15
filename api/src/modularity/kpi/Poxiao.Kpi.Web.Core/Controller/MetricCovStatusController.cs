namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 价值链状态接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-6.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "metric-covstatus", Name = "metric-covstatus", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricCovStatusController : IDynamicApiController
{
    private readonly IMetricCovStatusService _metricCovStatusService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricCovStatusService"></param>
    public MetricCovStatusController(IMetricCovStatusService MetricCovStatusService)
    {
        _metricCovStatusService = MetricCovStatusService;
    }

    /// <summary>
    /// 获取价值链状态信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricCovStatusService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取价值链状态列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<PagedResultDto<MetricCovStatusListOutput>> GetList([FromBody] MetricCovStatusListQueryInput input)
    {
        var list = await _metricCovStatusService.GetListAsync(input);
        return list;
    }

    /// <summary>
    /// 新建价值链状态.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricCovStatusCrInput input)
    {
        var isOk = await _metricCovStatusService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新价值链状态.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricCovStatusUpInput input)
    {
        input.Id = id;
        var isOk = await _metricCovStatusService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除价值链状态.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricCovStatusService.DeleteAsync(id);
        if (!isOk) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 获取价值链状态所有选项.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("options")]
    public async Task<List<MetricCovStatusOptionOutput>> GetOptionsAsync()
    {
        var list = await _metricCovStatusService.GetOptionsAsync();
        return list;
    }
}