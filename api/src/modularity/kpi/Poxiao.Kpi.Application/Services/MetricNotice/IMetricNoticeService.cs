namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标通知服务.
/// </summary>
public interface IMetricNoticeService
{
    /// <summary>
    /// 获取消息模板信息.
    /// </summary>
    /// <returns></returns>
    Task<List<MetricNoticeTemplateOutput>> GetTemplatesAsync();

    /// <summary>
    /// 获取通知列表.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<MetricNoticeOutput>> GetListAsync(MetricNoticeQryInput input);

    /// <summary>
    /// 创建消息通知.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<int> CreateAsync(MetricNoticeCrInput input);

    /// <summary>
    /// 删除消息通知.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<int> DeleteAsync(string id);
}
