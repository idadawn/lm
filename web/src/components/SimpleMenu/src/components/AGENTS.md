<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Inner primitives for `SimpleMenu`. `Menu` is the root container that owns the mitt-based event bus and `provide`s context to descendants; `MenuItem` and `SubMenuItem` are the leaf and branch renderers; `MenuCollapseTransition` animates submenu open/close.

## Key Files
| File | Description |
|------|-------------|
| `Menu.vue` | Root `<ul>`; props for theme/accordion/collapsed widths; emits `select` and `open-change`; provides `SubMenuProvider` context. |
| `SubMenuItem.vue` | Branch renderer — handles popover (collapsed sider) vs inline tree, hover/active styles, recursion. |
| `MenuItem.vue` | Leaf row — icon, label, route navigation, active state. |
| `MenuCollapseTransition.vue` | JS-driven height transition for submenu expand/collapse animation. |
| `menu.less` | Variables and rules for the menu theme. |
| `useMenu.ts` | Composable resolving root menu emitter + active state from `inject`. |
| `useSimpleMenuContext.ts` | `createSimpleRootMenuContext`/`useSimpleRootMenuContext` provide/inject pair. |
| `types.ts` | `SubMenuProvider` interface. |

## For AI Agents

### Working in this directory
- Communication between `Menu`, `SubMenuItem`, and `MenuItem` flows through `mitt()` (root emitter) — don't replace with prop drilling.
- Collapsed-mode submenus render as floating popovers; non-collapsed as inline accordion.
- `theme: 'light' | 'dark'` propagates via context — keep names exact.

## Dependencies
### Internal
- `/@/utils/mitt`, `/@/hooks/web/useDesign`, `/@/utils/propTypes`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
