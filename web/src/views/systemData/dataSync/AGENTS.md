<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataSync

## Purpose
数据同步 — one-shot/batch table-level sync from data source A → B. Choose two registered connections, validate, then sync individual tables or all selected tables.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Main page: From/To connection selectors, 验证连接, 规则配置, table list with per-row 数据同步 + 批量同步. |
| `Form.vue` | Rule-configuration form for sync mappings (column-level rules). |

## For AI Agents

### Working in this directory
- Pulls connection list via `getDataSourceSelector` from `dataSource` — keep `dbConnectionFrom` / `dbConnectionTo` symmetry.
- The 温馨提示 alert clarifies direction (A→B); preserve when editing UI to avoid user confusion.
- Per-row `btnLoading` flag must be reset after sync to keep the action column clickable.

## Dependencies
### Internal
- `/@/api/systemData/dataSync`, `/@/api/systemData/dataSource`
- `/@/components/Table`, `JnpfAlert`, `jnpf-select`
