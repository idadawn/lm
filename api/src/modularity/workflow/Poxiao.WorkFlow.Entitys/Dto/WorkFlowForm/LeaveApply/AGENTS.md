<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# LeaveApply

## Purpose
请假申请表单的入参 / 输出 DTO，对应 `WFORM_LEAVEAPPLY` 表与 `LeaveApplyService` 控制器。

## Key Files
| File | Description |
|------|-------------|
| `LeaveApplyInput.cs` | 请假申请入参：billNo/flowTitle/flowUrgent/leaveType/leaveReason/leaveStartTime/leaveEndTime/leaveDayCount/leaveHour/applyDept/applyPost/applyUser/applyDate/fileJson/flowId/status |
| `LeaveApplyInfoOutput.cs` | 请假申请详情输出（含创建/修改信息） |

## For AI Agents

### Working in this directory
- 字段必须与 `LeaveApplyEntity` 一一对应（实体使用 `F_*` 列名映射）。
- 时间字段用 `DateTime?`，前端传 ISO 字符串。

### Common patterns
- 表单顶部固定字段：`id / status / billNo / flowTitle / flowUrgent / flowId`。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Models.WorkFlow`（基类共用字段）、`DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
