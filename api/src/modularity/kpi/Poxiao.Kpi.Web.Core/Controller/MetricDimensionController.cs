namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 公共维度接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-6.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "metric-dimension", Name = "metric-dimension", Order = 202)]
[Route("api/kpi/v1/[controller]")]
public class MetricDimensionController : IDynamicApiController
{
    private readonly IMetricDimensionService _metricDimensionService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="MetricDimensionService"></param>
    public MetricDimensionController(IMetricDimensionService MetricDimensionService)
    {
        _metricDimensionService = MetricDimensionService;
    }

    /// <summary>
    /// 获取公共维度信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var info = await _metricDimensionService.GetAsync(id);
        return info;
    }

    /// <summary>
    /// 获取公共维度列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("list")]
    public async Task<PagedResultDto<MetricDimensionListOutput>> GetList([FromBody] MetricDimensionListQueryInput input)
    {
        var list = await _metricDimensionService.GetListAsync(input);
        return list;
    }

    /// <summary>
    /// 新建公共维度.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricDimensionCrInput input)
    {
        var isOk = await _metricDimensionService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新公共维度.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MetricDimensionUpInput input)
    {
        input.Id = id;
        var isOk = await _metricDimensionService.UpdateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除公共维度.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _metricDimensionService.DeleteAsync(id);
        if (!isOk) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 获取所有公共维度选项.
    /// </summary>
    /// <returns></returns>
    [HttpGet("options")]
    public async Task<List<MetricDimOptionsOutput>> GetOptions()
    {
        var list = await _metricDimensionService.GetOptionsAsync();
        return list;
    }

    /// <summary>
    /// 根据维度id查询维度数据.
    /// </summary>
    /// <param name="id">id.</param>
    /// <returns></returns>
    [HttpGet("data/{id}")]
    public async Task<ModelDataListOutput> GetDimensionDataAsync(string id)
    {
        var data = await _metricDimensionService.GetDimensionDataAsync(id);
        return data;
    }

}