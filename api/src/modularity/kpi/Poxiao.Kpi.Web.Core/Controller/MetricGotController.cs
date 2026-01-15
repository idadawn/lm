using Microsoft.AspNetCore.Authorization;

namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标思维图接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-20
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricGot", Name = "MetricGot", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricGotController : IDynamicApiController
{
    private readonly IMetricGotService _metricGotService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricGotService"></param>
    public MetricGotController(IMetricGotService MetricGotService)
    {
        _metricGotService = MetricGotService;
    }

    /// <summary>
    /// 获取指标思维图信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricGotService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取指标思维图列表.
    /// </summary>
    /// <param name="type">类别.</param>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("list/{type}")]
    public async Task<PagedResultDto<MetricGotListOutput>> GetList(GotType type, [FromBody] MetricGotListQueryInput input)
    {

        var list = await _metricGotService.GetListAsync(type, input);
        return list;
    }

    /// <summary>
    /// 新建指标思维图.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricGotCrInput input)
    {
        var isOk = await _metricGotService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新指标思维图.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricGotUpInput input)
    {
        input.Id = id;
        var isOk = await _metricGotService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除指标思维图.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricGotService.DeleteAsync(id);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}