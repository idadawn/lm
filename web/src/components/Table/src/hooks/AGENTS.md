<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
Composable behaviour for `BasicTable`. Each hook owns one concern (columns, dataSource, scroll, selection, pagination, expand, footer, header, form, style, custom row, scrollTo, loading) and is wired together inside `BasicTable.vue`. Public consumers integrate via `useTable()`.

## Key Files
| File | Description |
|------|-------------|
| `useTable.ts` | Public registration hook returning `[register, TableActionType]` for callers (reload/setProps/getColumns/setSelectedRows/etc.). |
| `useTableContext.ts` | `createTableContext` / `useTableContext` provide-inject pair — used by inner cells/settings. |
| `useDataSource.ts` | Data fetching, transforms, search-form merge, cursor-mode batching, mutations on `setTableData`/`reload`. |
| `useColumns.ts` | Column resolution, action column injection, indexColumn, column visibility/order persistence. |
| `usePagination.tsx` | Pagination state + UI customisation (TSX file). |
| `useTableScroll.ts` | Scroll height computation respecting page wrapper, header, footer, search form. |
| `useRowSelection.ts` | Selection state, exposed setters/getters, integration with row-click. |
| `useLoading.ts` | Simple loading flag bridge. |
| `useScrollTo.ts` | Programmatic scroll to row/index. |
| `useTableExpand.ts` | Expand/collapse tree rows. |
| `useTableFooter.ts` | Summary row computation. |
| `useTableForm.ts` | Search-form ↔ table coupling. |
| `useTableHeader.ts` | Header slot/style helpers. |
| `useTableStyle.ts` | Row class name and inline style helpers. |
| `useCustomRow.ts` | Row event delegation for click/dblclick/contextmenu. |

## For AI Agents

### Working in this directory
- Hooks expect raw refs/computed/context — they are not standalone; using one outside `BasicTable` is unsupported.
- `useTable()` is the only stable extension point; do not mutate the returned action proxy.

## Dependencies
### Internal
- `../types/*`, `../props.ts`, `/@/components/Form`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
