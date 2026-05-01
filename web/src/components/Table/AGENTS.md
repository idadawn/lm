<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Table

## Purpose
`BasicTable` — the primary data-grid component used across the LIMS frontend. Wraps `ant-design-vue` Table with built-in search form, pagination, column setting, row selection, scroll, editable cells, action column, image cell, and a cursor-based load-more mode. Backed by a comprehensive hooks set under `src/hooks`.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Public exports — `BasicTable`, `TableAction`, `EditTableHeaderIcon`, `TableImg`, types from `src/types/*`, `useTable` hook. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Implementation root — `BasicTable.vue`, props, componentMap, hooks/, components/, types/ (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Always interact with the table through `useTable()` rather than refs — it returns a stable `TableActionType` (reload/setProps/getColumns/etc.).
- 游标模式 (`isCursorMode`) renders its own load-more footer outside the Ant Table to avoid Ant's `watchEffect` recursion — preserve this carve-out.
- `EditRecordRow`, `BasicColumn`, and form schema types are part of the public surface; treat as semver-stable.

### Common patterns
- Hooks-based composition; each concern (columns, dataSource, scroll, selection, pagination) is its own composable.

## Dependencies
### Internal
- `/@/components/Form` (BasicForm/useForm), `/@/enums/pageEnum`.
### External
- `ant-design-vue` (`Table`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
