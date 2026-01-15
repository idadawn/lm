namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标分析接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-01-08.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "Metric", Name = "analysis", Order = 200)]
[Route("api/kpi/v1/metric/[controller]")]
public class MetricAnalysisController : IDynamicApiController
{
    private readonly IMetricAnalysisTaskService _analysisTaskService;

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="analysisTaskService"></param>
    public MetricAnalysisController(IMetricAnalysisTaskService analysisTaskService)
    {
        _analysisTaskService = analysisTaskService;
    }

    /// <summary>
    /// 创建分析任务.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("task")]
    public async Task<MetricAnalysisTaskOutput> CreateAsync([FromBody] MetricAnalysisTaskCrInput input)
    {
        var info = await _analysisTaskService.CreateAsync(input);
        return info;
    }

    /// <summary>
    /// 创建分析任务.
    /// </summary>
    /// <param name="taskId">taskId.</param>
    /// <returns></returns>
    [HttpGet("status/{taskId}")]
    public async Task<MetricAnalysisTaskStatusOutput> GetStatusAsync(string taskId)
    {
        var info = await _analysisTaskService.GetStatusAsync(taskId);
        return info;
    }

    /// <summary>
    /// 获取分析结果.
    /// </summary>
    /// <param name="taskId">请求参数.</param>
    /// <returns></returns>
    [HttpPost("summary/{taskId}")]
    public async Task<MetricAnalysisSummaryOutput> GetSummaryAsync(string taskId)
    {
        var info = await _analysisTaskService.GetSummaryAsync(taskId);
        return info;
    }


    /// <summary>
    /// 获取图表分析结果.
    /// </summary>
    /// <param name="taskId">请求参数.</param>
    /// <returns></returns>
    [HttpPost("result/{taskId}")]
    public async Task<MetricAnalysisResultOutput> GetResultAsync(string taskId)
    {
        var info = await _analysisTaskService.GetResultAsync(taskId);
        return info;
    }
}