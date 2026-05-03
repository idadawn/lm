<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# collectorManagement

## Purpose
采集器管理 module. Manages OPC/Modbus-style data collectors and the tags they expose. UI layer for the backend `collector` module — collector CRUD/enable/disable + config export, plus普通 tag and 逻辑 tag tables with history curve / table popups.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `collector/` | Collector list, dynamic-form add/edit, config save/export (see `collector/AGENTS.md`) |
| `tag/` | Tag editor with two tabs (普通标签 / 逻辑标签) and history viewers (see `tag/AGENTS.md`) |

## For AI Agents

### Working in this directory
- All API calls go through `/@/api/collector` — that file is the single source of HTTP endpoints.
- Add/edit forms use `z-temp-field` (a global custom component) driven by a `formState.elements` schema fetched from `collectorConfigTemplate` — do not hard-code form fields.
- The "保存配置" / "导出配置" buttons in `collector/index.vue` call `systemSaveConfig` / `systemDownloadCollectorConfig`. These mutate device-side state — guard with confirm dialogs.

### Common patterns
- `BasicTable` + custom `bodyCell` slots for `valueType` (decoded via `valueTypeFun`) and `action`.
- History data dialogs are nested `<a-modal>` widgets inside the page (not separate routes).

## Dependencies
### Internal
- `/@/api/collector`, `/@/components/{Table,Chart}`, custom global `z-temp-field`
### External
- `ant-design-vue`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
