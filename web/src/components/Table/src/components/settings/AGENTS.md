<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# settings

## Purpose
Toolbar buttons rendered next to the table title for end-user table configuration. Wired into `TableHeader` via `TableSetting` (the `index.vue` aggregator). Each setting is independently toggleable through the table's `setting` prop.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `TableSetting` aggregator — conditionally mounts each setting based on `getSetting.expand/redo/size/setting/fullScreen`; emits `columns-change`. |
| `ColumnSetting.vue` | Drag-orderable column visibility panel; supports fixed columns, drag handle, search; persists via the table action API. |
| `SizeSetting.vue` | Density picker (default/middle/small) writing back to `BasicTable` size prop. |
| `RedoSetting.vue` | Reload-data icon button. |
| `FullScreenSetting.vue` | Toggle browser-fullscreen on the table wrapper. |
| `ExpandSetting.vue` | "Expand all / collapse all" for tree tables. |

## For AI Agents

### Working in this directory
- Settings communicate with the table through `useTableContext()` — never reach into the parent SFC directly.
- `ExpandSetting` is only mounted when `getIsTreeTable` is true; respect that gate.
- 中文 tooltips/labels — fetched via `useI18n` `t(...)`.

## Dependencies
### Internal
- `../../hooks/useTableContext`, `../../types/table`, `/@/hooks/web/useI18n`, `/@/hooks/web/useDesign`.
### External
- `ant-design-vue`, `@ant-design/icons-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
