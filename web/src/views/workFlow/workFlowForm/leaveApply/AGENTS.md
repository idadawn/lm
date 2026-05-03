<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# leaveApply

## Purpose
内置请假申请表单 — flow-bound leave-request form with applicant/department fields and `flowTitle`/`billNo` headers.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Leave form with `dataForm.billNo`, `applyUser`, `applyDate`, `applyDept` etc. |

## For AI Agents

### Working in this directory
- Applicant/date are read-only — keep `disabled`/`readonly` attributes; values come from `useFlowForm` initialization.
- Wrap every field in `v-if="judgeShow(fieldName)"` and `:disabled="judgeWrite(fieldName)"` to honor node permissions.
