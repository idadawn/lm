namespace Poxiao.Lab.Entity.Dto.DeviceData;

/// <summary>
/// 设备数据批量上报入参.
/// </summary>
public class DeviceDataBatchInput
{
    /// <summary>
    /// 采集网关标识.
    /// </summary>
    public string CollectorId { get; set; }

    /// <summary>
    /// 设备编码（取值：stacking/ring-sample/single-sheet）.
    /// </summary>
    public string DeviceCode { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    public string BatchId { get; set; }

    /// <summary>
    /// 数据记录列表.
    /// </summary>
    public List<DeviceDataRecordInput> Records { get; set; } = new List<DeviceDataRecordInput>();
}

/// <summary>
/// 设备数据单条记录入参.
/// </summary>
public class DeviceDataRecordInput
{
    /// <summary>
    /// 采集端源库主键/自增ID，用于幂等去重.
    /// </summary>
    public string SourceKey { get; set; }

    /// <summary>
    /// 原始数据（JSON格式）.
    /// </summary>
    public string PayloadJson { get; set; }

    /// <summary>
    /// 采集时间（设备侧采集时间）.
    /// </summary>
    public DateTime? CollectedAt { get; set; }
}
