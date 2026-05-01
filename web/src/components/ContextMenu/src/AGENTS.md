<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Internals of the imperative right-click menu: the floating Vue component, the `createVNode`/`render` factory that mounts it under `document.body`, and shared types.

## Key Files
| File | Description |
|------|-------------|
| `ContextMenu.vue` | TSX component rendering `Menu` with `Menu.Item`/`Menu.SubMenu`, divider support, viewport-aware positioning (flips above/left when overflow), zIndex 9999. |
| `createContextMenu.ts` | Factory: builds a div container, `createVNode(contextMenuVue, propsData)`, attaches click/scroll listeners on `body` to auto-dismiss; returns a Promise resolved on close. Tracks open menus in `menuManager.domList`. |
| `typing.ts` | Types: `ContextMenuItem`, `Axis`, `CreateContextOptions`, `ContextMenuProps`, `ItemContentProps`. |

## For AI Agents

### Working in this directory
- Positioning logic in `getStyle` flips coordinates when the menu would overflow `body.clientWidth/clientHeight` — preserve when modifying.
- `destroyContextMenu` clears all tracked DOM nodes; do not bypass `menuManager` when teardown is needed.
- The factory short-circuits when `!isClient` (SSR-safe) — keep this guard.

### Common patterns
- Recursive `renderMenuItem` handles nested `children` via `Menu.SubMenu`.
- Each item's `handler()` is invoked from `handleAction` after `stopPropagation` + closing the menu.

## Dependencies
### Internal
- `/@/components/Icon`, `/@/utils/is` (isClient).
### External
- `ant-design-vue` (Menu, Divider), `vue` (createVNode/render/defineComponent).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
