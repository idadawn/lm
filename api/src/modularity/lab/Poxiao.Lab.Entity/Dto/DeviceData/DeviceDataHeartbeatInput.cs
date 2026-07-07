namespace Poxiao.Lab.Entity.Dto.DeviceData;

/// <summary>
/// 采集网关心跳上报入参.
/// </summary>
public class DeviceDataHeartbeatInput
{
    /// <summary>
    /// 采集网关标识.
    /// </summary>
    public string CollectorId { get; set; }

    /// <summary>
    /// 采集网关版本号.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 附加信息.
    /// </summary>
    public string Message { get; set; }
}
