<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HTableList

## Purpose
Generic data-list widget. Two modes: a regular `<a-table>` (`styleType==1`) with full column config, or a custom card list (`==2`/`==3`) showing `name` / optional `time` / optional `description` derived from configurable columns.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Uses `useTable(activeData)` for `getTableBindValues`, `list`, `getOption`, `getColumns`, `getItemStyle`, `getColumnsStyle`. Card mode shows column[0]/[1]/[2] depending on `styleType` and the `describe` flag. Falls back to the shared empty state. |

## For AI Agents

### Working in this directory
- Columns are referenced positionally (`getColumns[0..2]`) in card modes — keep at least three configured columns or the styles fall apart. The designer panel (`RTableSet.vue`) enforces this implicitly.
- `getBorder` toggles the `custom-table` LESS modifier in table mode; mirror it in any new style variants.
- `'describe'` is an option flag (not a CSS class) that decides whether the second column shows in `styleType==2`.

### Common patterns
- Hook destructure at top of `<script setup>`; no methods inline.

## Dependencies
### Internal
- `../../Design/hooks/useTable`, `../CardHeader`

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
