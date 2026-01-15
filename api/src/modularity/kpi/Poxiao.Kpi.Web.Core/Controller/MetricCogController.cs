namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标图链接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-20.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricCog", Name = "MetricCog", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricCogController : IDynamicApiController
{
    private readonly IMetricCogService _metricCogService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricCogService"></param>
    public MetricCogController(IMetricCogService MetricCogService)
    {
        _metricCogService = MetricCogService;
    }

    /// <summary>
    /// 获取指标图链信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricCogService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取指标图链列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<dynamic> GetList([FromBody] MetricCogListQueryInput input)
    {
        var list = await _metricCogService.GetListAsync(input);
        return list;
    }

    /// <summary>
    /// 新建指标图链.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricCogCrInput input)
    {
        var isOk = await _metricCogService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新指标图链.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricCogUpInput input)
    {
        input.Id = id;
        var isOk = await _metricCogService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除指标图链.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricCogService.DeleteAsync(id);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}