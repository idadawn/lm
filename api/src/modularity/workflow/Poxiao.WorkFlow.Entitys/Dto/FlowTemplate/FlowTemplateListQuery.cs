using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowTemplate;
[SuppressSniffer]
public class FlowTemplateListQuery : PageInputBase
{
    /// <summary>
    /// 流程分类.
    /// </summary>
    public string? category { get; set; }

    /// <summary>
    /// 开始时间.
    /// </summary>
    public long? startTime { get; set; }

    /// <summary>
    /// 结束时间.
    /// </summary>
    public long? endTime { get; set; }

    /// <summary>
    /// 0:发起流程 1:功能流程.
    /// </summary>
    public int? flowType { get; set; }

    /// <summary>
    /// 所属流程.
    /// </summary>
    public string? flowId { get; set; }
}
