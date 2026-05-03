<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# editable

## Purpose
Inline cell-edit support for `BasicTable`. Provides the always-rendered `EditableCell` SFC that switches between display and editor on click, the editor component dispatcher (`CellComponent`), and shared helpers/types for edit rows.

## Key Files
| File | Description |
|------|-------------|
| `EditableCell.vue` | Inline-edit cell with display/edit toggle, click-outside save, validation, async submit indicator (`Spin`), check/close icons. |
| `CellComponent.ts` | Renders the editor (Input/Select/Switch/DatePicker/…) using `componentMap`; binds value/events/props from column config. |
| `helper.ts` | `createPlaceholderMessage` placeholder generator for editor types. |
| `index.ts` | Re-exports `EditableCell`, `CellComponent`, types incl. `EditRecordRow`. |

## For AI Agents

### Working in this directory
- Edit flow: click triggers `editing=true`; `clickOutside` directive (`/@/directives/clickOutside`) fires save; cancel via close icon reverts to original.
- `EditRecordRow` is part of the public surface (re-exported through `Table/index.ts`) — do not narrow without bumping consumers.
- Picks props (`pick(column, [...])`) drive the editor's binding — keep prop names aligned with `componentMap` keys.

## Dependencies
### Internal
- `/@/directives/clickOutside`, `/@/hooks/web/useDesign`, `/@/utils/is`, `/@/utils/propTypes`, `../../hooks/useTableContext`, `../../types/table`.
### External
- `ant-design-vue` (`Spin`), `@ant-design/icons-vue`, `lodash-es` (`pick`, `set`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
