using Poxiao.Infrastructure.Filter;
using Poxiao.DependencyInjection;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowComment;

[SuppressSniffer]
public class FlowCommentListQuery : PageInputBase
{
    /// <summary>
    /// 任务id.
    /// </summary>
    public string? taskId { get; set; }
}

