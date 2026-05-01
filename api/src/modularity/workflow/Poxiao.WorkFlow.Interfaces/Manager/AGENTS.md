<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Manager

## Purpose
工作流业务编排接口。`IFlowTaskManager` 集中声明所有审批流转的入口，是 Service 层与具体实现之间的契约。

## Key Files
| File | Description |
|------|-------------|
| `IFlowTaskManager.cs` | 审批主接口：GetFlowBeforeInfo / Save / Submit / Audit / Reject / Recall / Revoke / Cancel / Assigned / Transfer / Press / Change / Suspend / Restore / GetCandidateModelList / NodeSelector / GetBatchCandidate / AdjustNodeByCon / IsSubFlowUpNode / GetBatchOperationData / Validation / RejectNodeList / NotifyEvent |

## For AI Agents

### Working in this directory
- 新增审批动作必须在此处定义方法，并在 `Poxiao.WorkFlow/Manager/FlowTaskManager.cs` 实现；签名一旦发布尽量保持向后兼容。
- 方法返回 `dynamic` 表示输出结构由前端期望决定（候选人 / 异常节点 / 表单数据等）；优先选择强类型返回。

### Common patterns
- 入参以 `FlowTaskParamter` / `FlowHandleModel` / `FlowTaskSubmitModel` 为主，统一审批上下文。
- XML 注释完整，逐参数说明用途。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Models.WorkFlow`、`Poxiao.WorkFlow.Entitys/Dto/FlowBefore`、`Entity`、`Model`、`Model/Properties`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
