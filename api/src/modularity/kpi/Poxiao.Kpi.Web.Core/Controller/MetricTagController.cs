namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 标签接口
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "Metric", Name = "MetricTag", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class MetricTagController : IDynamicApiController
{
    private readonly IMetricTagsService _metricTagsService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="metricTagsService"></param>
    public MetricTagController(IMetricTagsService metricTagsService)
    {
        _metricTagsService = metricTagsService;
    }

    /// <summary>
    /// 获取标签信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricTagsService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取标签列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<dynamic> GetList([FromBody] MetricTagsListQueryInput input)
    {
        var list = await _metricTagsService.GetListAsync(input);
        return list;
    }

    /// <summary>
    /// 获取指标下拉数据.
    /// </summary>
    /// <returns></returns>

    [HttpGet("selector")]
    public async Task<dynamic> GetSelector()
    {
        var list = await _metricTagsService.GetSelector();
        return list;
    }

    /// <summary>
    /// 新建标签.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricTagCrInput input)
    {
        var isOk = await _metricTagsService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新标签.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricTagUpInput input)
    {
        input.Id = id;
        var isOk = await _metricTagsService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除标签.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricTagsService.DeleteAsync(id);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

}