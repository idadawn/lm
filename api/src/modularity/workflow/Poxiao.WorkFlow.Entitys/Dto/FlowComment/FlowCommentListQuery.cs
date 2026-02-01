using Poxiao.DependencyInjection;
using Poxiao.Infrastructure.Filter;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowComment;

[SuppressSniffer]
public class FlowCommentListQuery : PageInputBase
{
    /// <summary>
    /// 任务id.
    /// </summary>
    public string? taskId { get; set; }
}

