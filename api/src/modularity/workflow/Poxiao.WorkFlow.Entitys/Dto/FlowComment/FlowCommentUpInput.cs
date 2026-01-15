using Poxiao.DependencyInjection;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowComment
{
    [SuppressSniffer]
    public class FlowCommentUpInput : FlowCommentCrInput
    {
        /// <summary>
        /// id.
        /// </summary>
        public string? id { get; set; }
    }
}
