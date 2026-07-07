namespace Poxiao.Lab.Entity.Dto.DeviceData;

/// <summary>
/// 设备数据暂存列表输出.
/// </summary>
public class DeviceDataInboxListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 设备编码.
    /// </summary>
    public string DeviceCode { get; set; }

    /// <summary>
    /// 采集网关标识.
    /// </summary>
    public string CollectorId { get; set; }

    /// <summary>
    /// 采集端源库主键/自增ID.
    /// </summary>
    public string SourceKey { get; set; }

    /// <summary>
    /// 原始数据（JSON格式）.
    /// </summary>
    public string PayloadJson { get; set; }

    /// <summary>
    /// 采集时间.
    /// </summary>
    public DateTime? CollectedAt { get; set; }

    /// <summary>
    /// 接收时间.
    /// </summary>
    public DateTime? ReceivedAt { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    public string BatchId { get; set; }

    /// <summary>
    /// 处理状态.
    /// </summary>
    public string ProcessStatus { get; set; }

    /// <summary>
    /// 处理信息.
    /// </summary>
    public string ProcessMessage { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? CreatorTime { get; set; }
}
