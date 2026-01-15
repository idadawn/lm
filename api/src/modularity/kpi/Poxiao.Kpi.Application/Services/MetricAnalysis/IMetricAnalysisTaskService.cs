namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标分析任务接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2024-1-9.
/// </summary>
public interface IMetricAnalysisTaskService
{
    /// <summary>
    /// 新建指标分析任务.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<MetricAnalysisTaskOutput> CreateAsync(MetricAnalysisTaskCrInput input);

    /// <summary>
    /// 更新指标分析任务.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns>成功数量.</returns>
    Task<int> UpdateAsync(MetricAnalysisTaskUpInput input);

    /// <summary>
    /// 根据任务Id获取分析状态.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    Task<MetricAnalysisTaskStatusOutput> GetStatusAsync(string taskId);

    /// <summary>
    /// 根据任务Id获取分析结果.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    Task<MetricAnalysisSummaryOutput> GetSummaryAsync(string taskId);

    /// <summary>
    /// 返回指标结果.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    Task<MetricAnalysisResultOutput> GetResultAsync(string taskId);
}