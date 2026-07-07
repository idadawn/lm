namespace Poxiao.Lab.Entity.Dto.DeviceData;

/// <summary>
/// 设备数据批量上报出参.
/// </summary>
public class DeviceDataBatchOutput
{
    /// <summary>
    /// 本次上报总数.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 实际接收（新增）数量.
    /// </summary>
    public int Accepted { get; set; }

    /// <summary>
    /// 重复（已存在，跳过）数量.
    /// </summary>
    public int Duplicated { get; set; }
}
