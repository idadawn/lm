namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标通知接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-11-28.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricNotice", Name = "metric-notice", Order = 209)]
[Route("api/kpi/v1/[controller]")]
public class MetricNoticeController : IDynamicApiController
{
    private readonly IMetricNoticeService _metricNoticeService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="metricNoticeService"></param>
    public MetricNoticeController(IMetricNoticeService metricNoticeService)
    {
        _metricNoticeService = metricNoticeService;
    }

    /// <summary>
    /// 获取通知模板
    /// </summary>
    /// <returns></returns>
    [HttpGet("template/list")]
    public async Task<List<MetricNoticeTemplateOutput>> GetTemplatesAsync()
    {
        var template = await _metricNoticeService.GetTemplatesAsync();
        return template;
    }

    /// <summary>
    /// 获取通知列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<PagedResultDto<MetricNoticeOutput>> GetListAsync([FromQuery] MetricNoticeQryInput input)
    {
        var list = await _metricNoticeService.GetListAsync(input);
        return list;
    }

    /// <summary>
    /// 新建指标通知.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task CreateAsync([FromBody] MetricNoticeCrInput input)
    {
        var isOk = await _metricNoticeService.CreateAsync(input);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 删除通知.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        var isOk = await _metricNoticeService.DeleteAsync(id);
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }
}
