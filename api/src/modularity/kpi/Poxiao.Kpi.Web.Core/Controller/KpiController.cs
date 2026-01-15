using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Poxiao.Extras.CollectiveOAuth.Enums;
using Poxiao.Infrastructure.Extension;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using Tea.Utils;

namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标对话接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-03-29.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "CHAT" }, Tag = "chat", Name = "chat", Order = 200)]
[Route("api/kpi/v1/[controller]")]
[AllowAnonymous]
public class ChatController : IDynamicApiController
{
    /// <summary>
    /// 指标信息.
    /// </summary>
    private readonly IMetricInfoService _metricInfoService;

    /// <summary>
    /// 指标数据.
    /// </summary>
    private readonly IMetricDataService _metricDataService;

    /// <summary>
    /// 日志.
    /// </summary>
    private readonly ILogger<ChatController> _logger;

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="metricInfoService"></param>
    public ChatController(IMetricInfoService metricInfoService, IMetricDataService metricDataService, ILogger<ChatController> logger)
    {
        _metricInfoService = metricInfoService;
        _metricDataService = metricDataService;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有指标信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("getall")]
    [NonUnify]
    public async Task<List<MetricInfoListForChatDto>> GetAllAsync()
    {
        return await _metricInfoService.GetAll4ChatAsync();
    }

    /// <summary>
    /// 获取指标数据.
    /// </summary>
    /// <param name="name">指标名称.</param>
    /// <param name="time">时间/日期.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("metric")]
    [NonUnify]
    public async Task<MetricDataForChatDto> GetMetricDataAsync([FromQuery][Required] string name, string? time)
    {
        var (start, end) = time.GetTimeRange();
        _logger.LogInformation($"指标名称:{name},开始时间:{start},结束时间:{end}");

        var metricInfo = await _metricInfoService.GetByNameAsync(name);

        if (metricInfo == null)
        {
            _logger.LogWarning($"未找到指标信息:{name}");
            throw Oops.Oh("400", "未找到对应指标信息");
        }

        var input = new MetricDataQryInput()
        {
            MetricId = metricInfo.Id,
            TimeDimension = null
        };

        var dataTask = await _metricDataService.GetChartDataAsync(input);
        var valueDataTask = await _metricDataService.GetDataAsync(metricInfo.Id);

        var info = new MetricDataForChatDto()
        {
            Data = dataTask.Data.List.Select(x => x.Value!.ParseToDecimal(0)).Join(';'),
            //XAxis = dataTask.Data.List.Select(x => x.Dimension.ToString()).Join(';'),
            value = $"{metricInfo.Name}值为:{valueDataTask.Data.Data}."
        };

        return info;
    }
}
