<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Inline modal contents shown by `tag.vue` / `tagLogic.vue` for inspecting a tag's history. Two views with the same time-range search header but different presentations.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `echartsLineArea/` | History curve viewer (`Chart` + tabs: 曲线 / 表格) (see `echartsLineArea/AGENTS.md`) |
| `historyTable/` | Pure table view of historical readings (see `historyTable/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Both components receive a `dataEchart` / `dataTable` prop with shape `{ id, date: [start, end] }`. Time format is `'YYYY/MM/DD HH:mm:ss'` (slashes, not dashes) — backend expects this.
- Search params are derived from `formState.datetime` and merged into `searchInfo` before reload. Keep this synchronisation when adding fields.

### Common patterns
- `defineOptions({ name: '...' })` per child for KeepAlive identity inside modals.

## Dependencies
### Internal
- `/@/api/collector` (`tagHistory`, `historyPage`), `/@/components/{Chart,Table}`
### External
- `ant-design-vue`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
