<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# columnAuthorize

## Purpose
列权限 drawer — manages column-level permission rules per module, supports batch add and a 数据连接 (connect to DB columns) flow when `type == '2'`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Drawer with table, 批量新增 and 数据连接 actions. |
| `Form.vue` | Single column permission form. |
| `BatchForm.vue` | Multi-row batch creation form. |

## For AI Agents

### Working in this directory
- Reuses `ConnectForm` from sibling `connectForm/`; pass through `dbLinkId`/module context unchanged.
- Reads `getDataSourceSelector` for connection picker — same pattern as `formAuthorize`.
