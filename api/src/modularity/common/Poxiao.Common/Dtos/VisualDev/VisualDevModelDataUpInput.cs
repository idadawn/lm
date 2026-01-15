namespace Poxiao.Infrastructure.Dtos.VisualDev;

/// <summary>
/// 在线功能开发数据修改输入.
/// </summary>
[SuppressSniffer]
public class VisualDevModelDataUpInput : VisualDevModelDataCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}
