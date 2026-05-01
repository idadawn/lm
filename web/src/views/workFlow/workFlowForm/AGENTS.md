<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# workFlowForm

## Purpose
内置流程业务表单 — built-in business forms hosted inside the workflow `FlowParser`. Each subdirectory is a self-contained form (leave, sales order, CRM order, plus a generic dynamic form).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `crmOrder/` | 客户订单 (CRM order) form (see `crmOrder/AGENTS.md`). |
| `dynamicForm/` | Generic dynamic form parser (see `dynamicForm/AGENTS.md`). |
| `hooks/` | Shared `useFlowForm` composable (see `hooks/AGENTS.md`). |
| `leaveApply/` | 请假申请 form (see `leaveApply/AGENTS.md`). |
| `salesOrder/` | 销售订单 form (see `salesOrder/AGENTS.md`). |

## For AI Agents

### Working in this directory
- All built-in forms accept a `config` prop and emit `setPageLoad` / `eventReceiver` — keep this contract when adding new forms.
- `useFlowForm` centralizes 节点/流程urgent state — adopt it instead of re-implementing per form.
