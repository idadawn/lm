<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
TypeScript surface for `BasicTable`. Defines the public API (column schema, table props, action handle, pagination, editable component types) that downstream views use to type their `useTable()` integration. Re-exported from `Table/index.ts`.

## Key Files
| File | Description |
|------|-------------|
| `table.ts` | Main types — `BasicTableProps`, `TableActionType`, `FetchSetting`, `TableSetting`, `SizeType`, `ColumnChangeParam`, `InnerHandlers`, etc. (largest module, 13k). |
| `column.ts` | `BasicColumn` (extends Ant `ColumnProps` with edit/auth/customRender/format/edit-rule fields). |
| `pagination.ts` | `PaginationProps` aliasing/extending Ant pagination config. |
| `tableAction.ts` | `ActionItem` (icon/label/popConfirm/modelConfirm/auth/onClick/tooltip), `ActionDropDown`. |
| `componentType.ts` | `ComponentType` union for editable cells (matches `componentMap` keys). |

## For AI Agents

### Working in this directory
- `ComponentType` and `componentMap.ts` keys must stay aligned — extending one without the other yields silent runtime fallback.
- `TableActionType` is the public action contract returned by `useTable()`; treat as semver-stable.
- `BasicColumn` `edit*` fields drive `EditableCell`; new edit features add fields here, not on Ant column type.

## Dependencies
### External
- `ant-design-vue` `Table` types.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
