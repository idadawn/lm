<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enum

## Purpose
工作流核心枚举集合。集中维护流程节点类型、任务状态、审批人类型，避免业务代码硬编码数字常量。

## Key Files
| File | Description |
|------|-------------|
| `FlowTaskNodeTypeEnum.cs` | 节点类型：start / approver / subFlow / condition / timer / end |
| `FlowTaskStatusEnum.cs` | 任务状态：Draft(0) / Handle(1) / Adopt(2) / Reject(3) / Revoke(4) / Cancel(5) |
| `FlowTaskOperatorEnum.cs` | 审批人类型：LaunchCharge(1) / DepartmentCharge(2) / InitiatorMe(3) / VariableApprover(4) / LinkApprover(5) / CandidateApprover(7) / ServiceApprover(9) / SubProcesses(10) |

## For AI Agents

### Working in this directory
- 枚举值必须打 `[Description("...")]`，前端通过描述显示中文。
- 枚举类标 `[SuppressSniffer]`，避免被自动 Sniffer 误注册。
- 修改枚举值时不要改动数字（数据库已存储数字），只能新增。

### Common patterns
- 数字与中文描述一一对应，描述短词中性，避免业务术语漂移。
- `FlowTaskOperatorEnum` 数字不连续（缺 6、8），保留历史兼容。

## Dependencies
### Internal
- `framework/Poxiao/DependencyInjection`（`SuppressSniffer`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
