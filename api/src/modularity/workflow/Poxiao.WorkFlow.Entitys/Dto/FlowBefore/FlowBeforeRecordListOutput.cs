using Poxiao.DependencyInjection;
using Poxiao.WorkFlow.Entitys.Model;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowBefore;

[SuppressSniffer]
public class FlowBeforeRecordListOutput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string? fullName { get; set; }

    /// <summary>
    /// 经办记录.
    /// </summary>
    public List<FlowBeforeRecordListModel> list { get; set; }
}