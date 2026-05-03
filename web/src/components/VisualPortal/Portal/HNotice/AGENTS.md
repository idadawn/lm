<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HNotice

## Purpose
Announcement / 公告 list widget. Two render modes: a flat `<a-table>` (`styleType == 1`) showing classify/title/time columns, or a card-style list with image, category badge, and click-through to detail.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Driven by `useTable(activeData)`; in table mode binds `getTableBindValues`, customizes the `fullName` cell with optional `【category】` prefix and a click-to-detail handler. Card mode iterates `list` with `getColumns`/`getItemStyle`/`getTypeStyle`. |

## For AI Agents

### Working in this directory
- The category field is `record.category`; gating uses `getOption.columnData.filter(o => o.filedName === 'classify')[0]?.show` — keep the field name `classify` aligned with the designer's `RNoticeColumnModal.vue`.
- The "click title" handler `readInfo(record)` opens a notice detail (project notice module); keep the navigation contract.
- `styleType==1` requires `getTableBindValues.columns?.length` — guard before render.

### Common patterns
- Reuses `useTable` so columns/data behave the same as `HTableList`.

## Dependencies
### Internal
- `../../Design/hooks/useTable`, `../CardHeader`

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
