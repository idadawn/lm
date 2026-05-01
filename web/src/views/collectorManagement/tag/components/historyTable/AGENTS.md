<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# historyTable

## Purpose
Tabular tag-history viewer rendered inside `tag.vue` / `tagLogic.vue` modals. Columns: ID / 名称 / 状态 / 值 / 时间. Time-range search via `RangePicker` defaulting to `dataTable.date`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `BasicTable` over `historyPage` with `searchInfo = {tagId, start, end}`; submit/reset re-call `reload()` |

## For AI Agents

### Working in this directory
- `searchInfo` is `reactive` and mutated in place by `submitForm()`. The `useTable` API picks up changes through the `:searchInfo` binding — don't replace with a new object reference.
- Time format `'YYYY/MM/DD HH:mm:ss'` (slashes) — must align with the backend and `echartsLineArea/index.vue`.
- The form has no validation rules — both endpoints tolerate identical start/end. Add rules only if backend behaviour changes.

### Common patterns
- `format: 'date|YYYY-MM-DD HH:mm'` on the `timeStamp` column uses the project's BasicTable formatter helper (note the dash format here is for *display*, slash format is for *query*).

## Dependencies
### Internal
- `/@/api/collector` (`historyPage`), `/@/components/{Table,Modal}`, `/@/hooks/web/useMessage`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
