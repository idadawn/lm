using Poxiao.DependencyInjection;
using Poxiao.Infrastructure.Models.WorkFlow;
using Poxiao.WorkFlow.Entitys.Model;
using Poxiao.WorkFlow.Entitys.Model.Properties;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowBefore;

[SuppressSniffer]
public class FlowBeforeInfoOutput
{
    /// <summary>
    /// 表单数据.
    /// </summary>
    public object formData { get; set; }

    /// <summary>
    /// 表单详情.
    /// </summary>
    public FlowFormModel flowFormInfo { get; set; }

    /// <summary>
    /// 流程详情.
    /// </summary>
    public FlowJsonModel flowTemplateInfo { get; set; }

    /// <summary>
    /// 流程任务.
    /// </summary>
    public FlowTaskModel? flowTaskInfo { get; set; }

    /// <summary>
    /// 流程任务节点.
    /// </summary>
    public List<FlowTaskNodeModel>? flowTaskNodeList { get; set; } = new List<FlowTaskNodeModel>() { };

    /// <summary>
    /// 流程任务经办.
    /// </summary>
    public List<FlowTaskOperatorModel>? flowTaskOperatorList { get; set; } = new List<FlowTaskOperatorModel>() { };

    /// <summary>
    /// 流程任务经办记录.
    /// </summary>
    public List<FlowTaskOperatorRecordModel>? flowTaskOperatorRecordList { get; set; } = new List<FlowTaskOperatorRecordModel>() { };

    /// <summary>
    /// 当前节点权限.
    /// </summary>
    public List<object> formOperates { get; set; } = new List<object>();

    /// <summary>
    /// 当前节点属性.
    /// </summary>
    public ApproversProperties approversProperties { get; set; } = new ApproversProperties();

    /// <summary>
    /// 审核保存数据.
    /// </summary>
    public object? draftData { get; set; }
}

