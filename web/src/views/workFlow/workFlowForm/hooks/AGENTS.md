<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
Composables shared by the built-in business forms in `workFlowForm/*`.

## Key Files
| File | Description |
|------|-------------|
| `useFlowForm.ts` | Generic init/submit/judge helpers — handles `selfState`, `formRef`, `tableRequiredData`, bill-number generation, and the `judgeShow`/`judgeWrite` permission helpers. |

## For AI Agents

### Working in this directory
- All business forms (`leaveApply`, `salesOrder`, `crmOrder`, etc.) opt into this composable. When adding a new form, accept the `UseFlowFormContext` shape rather than rolling your own.
- `getBillNumber` (from `/@/api/system/billRule`) is the canonical source for 流程编码 — do not generate locally.

## Dependencies
### Internal
- `/@/store/modules/user`, `/@/api/system/billRule`, `/@/hooks/web/useMessage`, `/@/utils/is`
### External
- `ant-design-vue` (`FormInstance`)
