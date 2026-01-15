using Poxiao.DependencyInjection;
using Poxiao.WorkFlow.Entitys.Entity;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowTemplate;

[SuppressSniffer]
public class FlowTemplateImportOutput
{
    /// <summary>
    /// 流程实例.
    /// </summary>
    public FlowTemplateEntity flowTemplate { get; set; }

    /// <summary>
    /// 流程实例.
    /// </summary>
    public List<FlowTemplateJsonEntity> flowTemplateJson { get; set; }

    /// <summary>
    /// 流程可见范围.
    /// </summary>
    public List<FlowEngineVisibleEntity> visibleList { get; set; } = new List<FlowEngineVisibleEntity>();

    /// <summary>
    /// 流程关联表单.
    /// </summary>
    public List<FlowFormRelationEntity> formRelationList { get; set; } = new List<FlowFormRelationEntity>();
}
