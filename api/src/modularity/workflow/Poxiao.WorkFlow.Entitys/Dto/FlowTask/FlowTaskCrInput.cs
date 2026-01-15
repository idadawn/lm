using Poxiao.Infrastructure.Models.WorkFlow;
using Poxiao.DependencyInjection;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowTask;

[SuppressSniffer]
public class FlowTaskCrInput : FlowTaskOtherModel
{
    /// <summary>
    /// 引擎id.
    /// </summary>
    public string? flowId { get; set; }

    /// <summary>
    /// 主键id.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 表单数据.
    /// </summary>
    public object? formData { get; set; }

    /// <summary>
    /// 提交/保存 0-1.
    /// </summary>
    public int? status { get; set; }

    /// <summary>
    /// 紧急程度.
    /// </summary>
    public int? flowUrgent { get; set; } = 1;
}
