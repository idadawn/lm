using Poxiao.DependencyInjection;
using Poxiao.WorkFlow.Entitys.Entity;

namespace Poxiao.WorkFlow.Entitys.Dto.FlowEngine;

[SuppressSniffer]
public class FlowEngineImportOutput
{
    /// <summary>
    /// 流程实例.
    /// </summary>
    public FlowEngineEntity flowEngine { get; set; }

    /// <summary>
    /// 流程可见范围.
    /// </summary>
    public List<FlowEngineVisibleEntity> visibleList { get; set; } = new List<FlowEngineVisibleEntity>();
}
