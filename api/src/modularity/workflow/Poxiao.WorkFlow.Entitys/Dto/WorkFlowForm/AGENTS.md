<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# WorkFlowForm (Dto)

## Purpose
内置业务表单的 DTO 包装目录。当前包含两个示例：请假申请、销售订单。每个表单一个子目录，含 `*Input.cs` 与 `*InfoOutput.cs`。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `LeaveApply/` | 请假申请 DTO (see `LeaveApply/AGENTS.md`) |
| `SalesOrder/` | 销售订单 DTO（含 entryList 明细） (see `SalesOrder/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增内置表单：在此目录下建立子目录 + Input/InfoOutput 配对；并在 `Entity/WorkFlowForm/` 添加对应实体；Service 在 `Poxiao.WorkFlow/WorkFlowForm/` 实现。
- 字段命名 camelCase；保留 `flowId / flowTitle / flowUrgent / billNo / status` 等通用流程关联字段。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Models.WorkFlow`（`FlowTaskOtherModel`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
