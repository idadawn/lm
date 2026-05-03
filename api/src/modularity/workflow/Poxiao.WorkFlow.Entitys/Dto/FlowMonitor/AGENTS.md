<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowMonitor

## Purpose
流程监控（管理员视角）相关 DTO，被 `FlowMonitorService` 与 `FlowTaskRepository.GetMonitorList` 使用。支持按发起人 / 分类 / 模板 / 状态 / 紧急度 / 时间区间筛选。

## Key Files
| File | Description |
|------|-------------|
| `FlowMonitorListQuery.cs` | 监控列表查询：creatorUserId/flowCategory/templateId/flowId/startTime/endTime/status/flowUrgent |
| `FlowMonitorListOutput.cs` | 列表输出条目（任务 + 当前节点 + 处理人 + 紧急度） |
| `FlowMonitorDeleteInput.cs` | 强制删除流程入参（taskIds / 是否清记录） |

## For AI Agents

### Working in this directory
- 监控属于管理员能力，权限与可见性由 Service/Manager 层判断；DTO 不承载权限信息。
- 删除入参里勾选项需谨慎修改，避免误删历史归档。

### Common patterns
- 与 FlowBeforeListQuery 字段相似，复用前端组件方便。

## Dependencies
### Internal
- `framework/Poxiao/Infrastructure.Filter`（`PageInputBase`）、`DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
