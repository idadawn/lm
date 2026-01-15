using Aop.Api.Domain;
using Poxiao.Systems.Entitys.Dto.DbLink;

namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 派生指标定义接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-12.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "metric-derive", Name = "metric-derive", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricInfo4DeriveController : IDynamicApiController
{
    private readonly IMetricInfo4DeriveService _metricInfoService;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MetricInfo4DeriveController(IMetricInfo4DeriveService metricInfoService)
    {
        _metricInfoService = metricInfoService;
    }

    /// <summary>
    /// 获取指标定义信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricInfoService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 新建指标定义.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricInfo4DeriveCrInput input)
    {
        var isOk = await _metricInfoService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新指标定义.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricInfo4DeriveUpInput input)
    {
        input.Id = id;
        var isOk = await _metricInfoService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

}