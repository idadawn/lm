<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# buttonAuthorize

## Purpose
按钮权限 drawer — manages button-level permission entries scoped to a subsystem module, with quick-add from a 常用按钮权限 list.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Drawer with table + 常用按钮 dropdown for batch-creating common button permissions. |
| `Form.vue` | Single button permission edit form. |

## For AI Agents

### Working in this directory
- Uses `getButtonAuthorizeList`/`del`/`create` from `/@/api/system/buttonAuthorize`.
- The 常用按钮 quick-add calls `create` directly with templates — preserve the loading flag.
