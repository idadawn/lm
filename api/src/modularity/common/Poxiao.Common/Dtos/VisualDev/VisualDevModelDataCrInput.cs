using Poxiao.Infrastructure.Models.WorkFlow;

namespace Poxiao.Infrastructure.Dtos.VisualDev;

/// <summary>
/// 在线功能开发数据创建输入.
/// </summary>
[SuppressSniffer]
public class VisualDevModelDataCrInput: FlowTaskOtherModel
{
    /// <summary>
    /// 数据.
    /// </summary>
    public string data { get; set; }

    /// <summary>
    /// 1-保存.
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 紧急程度.
    /// </summary>
    public int? flowUrgent { get; set; } = 1;
}
