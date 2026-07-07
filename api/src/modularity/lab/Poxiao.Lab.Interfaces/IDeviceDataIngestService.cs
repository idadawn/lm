using Poxiao.Lab.Entity.Dto.DeviceData;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 设备数据采集接入服务接口.
/// </summary>
public interface IDeviceDataIngestService
{
    /// <summary>
    /// 批量上报设备原始数据（接收+暂存，按 SourceKey 幂等去重）.
    /// </summary>
    /// <param name="input">批量上报入参.</param>
    /// <returns></returns>
    Task<DeviceDataBatchOutput> Batch(DeviceDataBatchInput input);

    /// <summary>
    /// 采集网关心跳上报.
    /// </summary>
    /// <param name="input">心跳入参.</param>
    /// <returns></returns>
    Task<dynamic> Heartbeat(DeviceDataHeartbeatInput input);

    /// <summary>
    /// 获取暂存数据列表.
    /// </summary>
    /// <param name="input">查询参数.</param>
    /// <returns></returns>
    Task<dynamic> GetList(DeviceDataInboxListQuery input);
}
