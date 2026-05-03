<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation root for `BasicTable`. Combines the search form, ant-design-vue `<Table>`, custom header/footer cells, settings toolbar, and a special cursor-mode footer. Behaviour is decomposed into hooks under `hooks/` while UI fragments live under `components/`.

## Key Files
| File | Description |
|------|-------------|
| `BasicTable.vue` | Root SFC; wires `useColumns`, `useDataSource`, `useLoading`, `useRowSelection`, `useTableScroll`, `usePagination`, etc.; exposes `TableActionType` via context. |
| `props.ts` | Centralised props definition for `BasicTable` (search form, pagination, scroll, columns, etc.). |
| `componentMap.ts` | Map of editable-cell component names → ant-design-vue components (`Input`, `Select`, `Switch`, `DatePicker`…); `add`/`del` allow registering custom editors. |
| `const.ts` | Pagination defaults / shared constants. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Header/Footer/Action/Img cells + editable + settings (see `components/AGENTS.md`). |
| `hooks/` | Composable behaviour (data, columns, scroll, selection, pagination…) (see `hooks/AGENTS.md`). |
| `types/` | Type definitions (`table.ts`, `column.ts`, `pagination.ts`, `tableAction.ts`) (see `types/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `componentMap` is the extension point for editable cells — register custom editors there, not by patching `EditableCell.vue`.
- The cursor-mode load-more indicator and footer are intentionally rendered outside the `<Table>` to dodge `watchEffect` cascades — keep them outside.

## Dependencies
### Internal
- `/@/components/Form`, `/@/enums/pageEnum`.
### External
- `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
