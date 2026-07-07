using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Contracts;
using SqlSugar;

namespace Poxiao.Lab.Entity;

/// <summary>
/// 设备数据采集暂存表（接收采集网关上报的原始数据，暂不流入导入管线）.
/// </summary>
[SugarTable("LAB_DEVICE_DATA_INBOX")]
[Tenant(ClaimConst.TENANTID)]
public class DeviceDataInboxEntity : CLDEntityBase
{
    /// <summary>
    /// 设备编码（取值：stacking/ring-sample/single-sheet）.
    /// </summary>
    [SugarColumn(ColumnName = "F_DEVICE_CODE", Length = 50)]
    public string DeviceCode { get; set; }

    /// <summary>
    /// 采集网关标识.
    /// </summary>
    [SugarColumn(ColumnName = "F_COLLECTOR_ID", Length = 100)]
    public string CollectorId { get; set; }

    /// <summary>
    /// 采集端源库主键/自增ID，用于幂等去重.
    /// </summary>
    [SugarColumn(ColumnName = "F_SOURCE_KEY", Length = 200)]
    public string SourceKey { get; set; }

    /// <summary>
    /// 原始数据（JSON格式）.
    /// </summary>
    [SugarColumn(
        ColumnName = "F_PAYLOAD_JSON",
        ColumnDataType = StaticConfig.CodeFirst_BigString,
        IsNullable = true
    )]
    public string PayloadJson { get; set; }

    /// <summary>
    /// 采集时间（设备侧采集时间）.
    /// </summary>
    [SugarColumn(ColumnName = "F_COLLECTED_AT", IsNullable = true)]
    public DateTime? CollectedAt { get; set; }

    /// <summary>
    /// 接收时间（服务端接收时间）.
    /// </summary>
    [SugarColumn(ColumnName = "F_RECEIVED_AT", IsNullable = true)]
    public DateTime? ReceivedAt { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    [SugarColumn(ColumnName = "F_BATCH_ID", Length = 50, IsNullable = true)]
    public string BatchId { get; set; }

    /// <summary>
    /// 处理状态（pending/processed/failed/ignored）.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROCESS_STATUS", Length = 20)]
    public string ProcessStatus { get; set; } = "pending";

    /// <summary>
    /// 处理信息.
    /// </summary>
    [SugarColumn(ColumnName = "F_PROCESS_MESSAGE", Length = 500, IsNullable = true)]
    public string ProcessMessage { get; set; }
}
