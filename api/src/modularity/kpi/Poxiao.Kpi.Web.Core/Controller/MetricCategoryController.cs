namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标分类接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-10.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "Metric", Name = "MetricCategory", Order = 201)]
[Route("api/kpi/v1/[controller]")]
public class MetricCategoryController : IDynamicApiController
{
    private readonly IMetricCategoryService _metricCategoryService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricCategoryService"></param>
    public MetricCategoryController(IMetricCategoryService MetricCategoryService)
    {
        _metricCategoryService = MetricCategoryService;
    }

    /// <summary>
    /// 获取指标分类信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricCategoryService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取指标分类列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<dynamic> GetList([FromQuery] MetricCategoryListQueryInput input)
    {
        var list = await _metricCategoryService.GetListAsync(input);
        return list;
    }

    /// <summary>
    /// 新建指标分类.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricCategoryCrInput input)
    {
        var isOk = await _metricCategoryService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新指标分类.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricCategoryUpInput input)
    {
        input.Id = id;
        var isOk = await _metricCategoryService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除指标分类.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricCategoryService.DeleteAsync(id);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 获取指标分类树或者下拉.
    /// </summary>
    /// <returns></returns>
    [HttpGet("selector")]
    public async Task<dynamic> GetSelector()
    {
        var list = await _metricCategoryService.GetSelector();
        return list;
    }
}