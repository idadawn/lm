<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# formAuthorize

## Purpose
表单权限 drawer — manages form-field permission rules per module, parallel structure to `columnAuthorize` (single + batch + connect).

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Drawer with table, 批量新增 and 数据连接 actions. |
| `Form.vue` | Single form-field permission edit form. |
| `BatchForm.vue` | Multi-row batch form. |

## For AI Agents

### Working in this directory
- Mirror `columnAuthorize` semantics; keep API surface (`getFormAuthorizeList`/`del`) consistent with it.
- Share `ConnectForm` from `../connectForm/`; do not fork.
