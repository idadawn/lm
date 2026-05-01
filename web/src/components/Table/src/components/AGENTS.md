<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Cell-level and toolbar-level UI fragments composed by `BasicTable`. Includes the action button column, image column, header/title/footer renderers, edit-icon hint, and a dedicated cursor-pagination footer that lives outside the Ant table tree.

## Key Files
| File | Description |
|------|-------------|
| `TableAction.vue` | Action column — renders a list of `ActionItem` (icon/label/permissions/popConfirm/modelConfirm/tooltip), with a `Dropdown` overflow for `dropDownActions`. |
| `TableImg.vue` | Image cell with preview, multiple-image stacking, fallback. |
| `HeaderCell.vue` | Custom header cell renderer used when consumers don't override `headerCell` slot. |
| `TableHeader.vue` | Top toolbar (title + settings) above the table. |
| `TableFooter.vue` | Summary row footer driven by `useTableFooter` hook. |
| `CursorFooter.vue` | Floating "加载更多/没有更多了" footer for `isCursorMode`; exports `CURSOR_STATE_KEY` injection key. |
| `TableTitle.vue` | Title slot wrapper. |
| `EditTableHeaderIcon.vue` | Tiny pencil icon shown on editable column headers. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `editable/` | Inline cell edit (`EditableCell.vue`, `CellComponent.ts`, helpers) (see `editable/AGENTS.md`). |
| `settings/` | Column visibility, size, redo, fullscreen, expand toolbar (see `settings/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `TableAction` reads permissions/auth and respects `popConfirm`/`modelConfirm` — both confirm flavours coexist; pick based on action danger level.
- `CURSOR_STATE_KEY` is the cross-component contract between `BasicTable.vue` and `CursorFooter.vue` — import the constant, do not hardcode strings.

## Dependencies
### Internal
- `/@/components/Modal`, `/@/components/Tooltip`, `/@/hooks/web/useDesign`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
