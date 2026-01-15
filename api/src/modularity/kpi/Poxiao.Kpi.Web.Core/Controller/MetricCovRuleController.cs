namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标价值链规则接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-20.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricCovRule", Name = "MetricCovRule", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricCovRuleController : IDynamicApiController
{
    private readonly IMetricCovRuleService _metricCovRuleService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricCovRuleService"></param>
    public MetricCovRuleController(IMetricCovRuleService MetricCovRuleService)
    {
        _metricCovRuleService = MetricCovRuleService;
    }

    /// <summary>
    /// 获取指标价值链规则信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricCovRuleService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取指标价值链规则列表.
    /// </summary>
    /// <param name="covId">节点id.</param>
    /// <returns></returns>
    [HttpGet("list/{covId}")]
    public async Task<dynamic> GetList(string covId)
    {
        var list = await _metricCovRuleService.GetListAsync(covId);
        return list;
    }

    /// <summary>
    /// 新建指标价值链规则.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricCovRuleCrInput input)
    {
        var isOk = await _metricCovRuleService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新指标价值链规则.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricCovRuleUpInput input)
    {
        input.Id = id;
        var isOk = await _metricCovRuleService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除指标价值链规则.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricCovRuleService.DeleteAsync(id);
        if (!isOk) throw Oops.Oh(ErrorCode.COM1002);
    }
}