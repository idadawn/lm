using Poxiao.Infrastructure.Filter;

namespace Poxiao.Lab.Entity.Dto.DeviceData;

/// <summary>
/// 设备数据暂存列表查询参数.
/// </summary>
public class DeviceDataInboxListQuery : PageInputBase
{
    /// <summary>
    /// 设备编码.
    /// </summary>
    public string DeviceCode { get; set; }

    /// <summary>
    /// 采集网关标识.
    /// </summary>
    public string CollectorId { get; set; }

    /// <summary>
    /// 处理状态（pending/processed/failed/ignored）.
    /// </summary>
    public string ProcessStatus { get; set; }

    /// <summary>
    /// 开始日期（按接收时间过滤）.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期（按接收时间过滤）.
    /// </summary>
    public DateTime? EndDate { get; set; }
}
