<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# menu

## Purpose
子系统菜单管理 popup — Web/App-tabbed menu tree for the selected subsystem with import (.json template) and CRUD.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `BasicPopup` with Web/App tabs, search, table and import. |
| `Form.vue` | Menu node edit form (icon, parent, route, sort, enabledMark). |

## For AI Agents

### Working in this directory
- Search filters route through `searchInfo` keyed by `listQuery.category` (Web|App) — preserve that switching logic.
- Imported menu templates accept JSON only; do not change the `uploadTpl` action without updating the template format.
