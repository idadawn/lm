<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Header strips used by the two tree variants. Both render a title plus a toolbar dropdown that emits `ToolbarEnum` actions (reload, expand all, select all, strict toggle).

## Key Files
| File | Description |
|------|-------------|
| `TreeHeader.vue` | Header for `BasicTree`: optional title via `BasicTitle`, inline `InputSearch` (debounced via `@vueuse/core`), `Dropdown` with toolbar items keyed by `ToolbarEnum`. |
| `LeftTreeHeader.vue` | Header for `BasicLeftTree`: uses `BasicCaption` for title + action slot, supports both built-in `toolbarList` and custom `dropDownActions`, no inline search (search lives in parent). |

## For AI Agents

### Working in this directory
- Both files emit `strictly-change`, `reload` (and search-related events) — do not rename without updating consumers in `BasicTree.vue` / `BasicLeftTree.vue`.
- Toolbar item visibility is computed from props (`checkable`, `search`, `isAsync`, `expandAll`, `checkAll`); preserve the `ToolbarEnum`-keyed `value` field on each item.
- I18n keys come from `useI18n()` under `common.*` (e.g. `common.searchText`).

### Common patterns
- BEM via `createBEM('tree-header')` / parent's `basic-left-tree`.
- Iconography uses `icon-ym icon-ym-*` classes (custom Yummy icon font) and `ion:ellipsis-vertical` via `Icon`.

## Dependencies
### Internal
- `/@/components/Basic` (`BasicTitle`, `BasicCaption`)
- `/@/components/Icon`, `/@/hooks/web/useI18n`, `/@/utils/bem`
- `../types/tree` for `ToolbarEnum`, `DropDownActionItem`

### External
- `ant-design-vue` (Dropdown, Menu, MenuItem, MenuDivider, InputSearch)
- `@vueuse/core` (`useDebounceFn`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
