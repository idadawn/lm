<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowBefore

## Purpose
「待我审批 / 已审批」前置流程相关的请求与响应 DTO。被 `FlowBeforeService` 与 `FlowTaskRepository.GetWaitList / GetTrialList / GetBatchWaitList / GetCirculateList` 等方法使用。

## Key Files
| File | Description |
|------|-------------|
| `FlowBeforeListQuery.cs` | 待办/已办/抄送/批量审批共用列表查询；含 startTime/endTime/templateId/flowId/flowCategory/creatorUserId/nodeCode/flowUrgent |
| `FlowBeforeListOutput.cs` | 待办/已办列表输出条目（任务摘要 + 当前节点 + 紧急度） |
| `FlowBeforeInfoOutput.cs` | 任务详情：表单数据、节点轨迹、操作权限、按钮可见性 |
| `FlowBeforeRecordListOutput.cs` | 审批记录条目，按节点分类展示 |

## For AI Agents

### Working in this directory
- `FlowBeforeListQuery` 继承 `PageInputBase`，新增筛选字段时保持可空类型与 camelCase。
- `FlowBeforeInfoOutput` 是详情聚合结构，新增字段需要同步更新 `FlowTaskRepository` / `FlowTaskManager.GetFlowBeforeInfo`。

### Common patterns
- 列表查询字段命名与 `FlowLaunchListQuery` / `FlowMonitorListQuery` 对齐，便于前端复用。
- 时间使用 `long?`（毫秒/秒时间戳），由前端传入。

## Dependencies
### Internal
- `framework/Poxiao/Infrastructure.Filter`（`PageInputBase`）、`DependencyInjection`（`SuppressSniffer`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
