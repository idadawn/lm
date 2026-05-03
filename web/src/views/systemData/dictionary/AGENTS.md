<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dictionary

## Purpose
字典管理 — system-wide enum/dictionary entries grouped by 字典分类 tree. Powers dropdowns and lookups across the app.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tree (字典分类) + table of dictionary items. |
| `Form.vue` | Item edit form (code, name, sort, parent). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 字典分类 (type) management drawer (see `components/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Tree node selection drives `searchInfo` for the table — preserve the `handleTreeSelect` → reload flow.
- `leftDropDownActions` on the tree exposes 类型管理; this opens the components/ drawer.

## Dependencies
### Internal
- `/@/api/systemData/dictionary`, `/@/components/Tree`, `/@/components/Drawer`
