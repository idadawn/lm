<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# collector

## Purpose
Collector ("采集器") master list. Lists, paginates, adds, edits, enables/disables, saves, and exports collector configuration. Add/edit form is fully dynamic — `formState.elements` is loaded from `collectorConfigTemplate(type)` and rendered by the project-wide `z-temp-field` schema component.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Page entry — `BasicTable` over `collectorPage`, toolbar 添加/保存配置/导出配置, modal form with `formState.{type,name,description,elements}`, calls `collectorAdd/Update/Remove/Detail/Enable/Disable` and `systemSaveConfig`/`systemDownloadCollectorConfig`; type options come from `collectorDropDown` |

## For AI Agents

### Working in this directory
- The "type" select drives the dynamic form: on `@select` it calls `collectorConfigTemplate(type)` and replaces `formState.elements`. Preserve this — schema cannot be cached across types.
- `saveHandle()` and `exportHandle()` operate on the entire collector config (not the current row) — keep them as toolbar-level actions.
- Form validation uses Ant Design's `rules` on the static fields (type/name/description); element-level required checks live in `z-temp-field` via `requiredProps="isReq"`.

### Common patterns
- `<z-temp-field>` props: `typeProps="t"`, `labelProps="title"`, `keyProps="v"`, `listProps="options"`, `listValueType="index"` — must match the backend element shape.

## Dependencies
### Internal
- `/@/api/collector` (`collectorDropDown`, `collectorPage`, `collectorConfigTemplate`, `collectorAdd`, `collectorUpdate`, `collectorRemove`, `collectorDetail`, `collectorEnable`, `collectorDisable`, `systemSaveConfig`, `systemDownloadCollectorConfig`), `/@/components/Table`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
