<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowLaunch

## Purpose
「我发起的流程」相关 DTO。支持发起列表查询、列表输出与撤回操作入参。被 `FlowLaunchService` 与 `FlowTaskRepository.GetLaunchList` 使用。

## Key Files
| File | Description |
|------|-------------|
| `FlowLaunchListQuery.cs` | 列表查询：flowCategory/templateId/flowId/startTime/endTime/status/flowUrgent/delegateType（是否委托发起） |
| `FlowLaunchListOutput.cs` | 列表输出（任务摘要 + 当前节点 + 状态 + 是否可撤回） |
| `FlowLaunchActionWithdrawInput.cs` | 流程撤回入参（taskId/handleOpinion 等） |

## For AI Agents

### Working in this directory
- `delegateType` 用于区分「我自己发起」与「我代他人发起」，前端筛选用。
- 撤回动作仅在 `status == 1`（处理中）且当前用户为发起人时可用，业务校验在 Manager 层完成。

### Common patterns
- 时间使用 `long?` 时间戳，与 FlowBefore/FlowMonitor 一致。

## Dependencies
### Internal
- `framework/Poxiao/Infrastructure.Filter`（`PageInputBase`）、`DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
