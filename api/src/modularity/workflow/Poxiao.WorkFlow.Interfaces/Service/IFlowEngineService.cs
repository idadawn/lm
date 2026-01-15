using Poxiao.WorkFlow.Entitys.Dto.FlowEngine;

namespace Poxiao.WorkFlow.Interfaces.Service;

/// <summary>
/// 流程设计.
/// </summary>
public interface IFlowEngineService
{
    /// <summary>
    /// 发起列表.
    /// </summary>
    /// <returns></returns>
    Task<List<FlowEngineListOutput>> GetFlowFormList();
}
