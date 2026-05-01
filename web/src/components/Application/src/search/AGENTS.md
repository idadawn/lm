<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# search

## Purpose
Global menu/keyword search overlay: a teleported modal that fuzzy-matches menu entries via `useMenuSearch`, renders results as a keyboard-navigable list, and routes to the selected page on Enter.

## Key Files
| File | Description |
|------|-------------|
| `AppSearch.vue` | Trigger button (icon) shown in headers; toggles the modal's visibility. |
| `AppSearchModal.vue` | Teleported modal: input, result list, empty-state, click-outside close, keyboard nav. |
| `AppSearchKeyItem.vue` | Single result row showing menu path/icon. |
| `AppSearchFooter.vue` | Hint footer ("Enter / Esc / arrow keys"). |
| `useMenuSearch.ts` | Composable that flattens permission menus and filters by keyword. |

## For AI Agents

### Working in this directory
- The modal uses `<Teleport to="body">` and `v-click-outside` (from `/@/components/ClickOutSide`) — keep z-index/animation consistent with existing CSS class prefix.
- Search source comes from permission store menus; do not hard-code menu trees.
- i18n keys live under `common.*` and `component.app.search*`.

### Common patterns
- Class prefix via `useDesign('app-search')`.
- Keyboard handling delegated to refs collected by `setRefs(index)`.

## Dependencies
### Internal
- `/@/components/ClickOutSide`, `/@/hooks/web/useDesign`, permission/menu store, i18n via `useI18n`.
### External
- `ant-design-vue` (`a-input`), `@ant-design/icons-vue` (`SearchOutlined`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
