<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Top-level building blocks for `SimpleMenu`. Composes the inner `Menu` primitive with route-aware logic, recursive sub-menu rendering, and an open-keys hook tied to accordion mode. Style is co-located in `index.less`.

## Key Files
| File | Description |
|------|-------------|
| `SimpleMenu.vue` | Public component; iterates `items: MenuType[]` and delegates each to `SimpleSubMenu`; binds active/openKeys to current route. |
| `SimpleSubMenu.vue` | Recursive renderer; chooses between leaf `MenuItem` and nested `SubMenuItem` based on `children`. |
| `SimpleMenuTag.vue` | Badge/tag rendered next to a menu label (`type`, `dot`, `content`). |
| `useOpenKeys.ts` | Computes/maintains opened submenu keys; honours accordion + collapsed states. |
| `types.ts` | `MenuState` interface. |
| `index.less` | Theme tokens for light/dark menu surfaces. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Inner `Menu`/`MenuItem`/`SubMenuItem` primitives + collapse transition (see `components/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `accordion: true` is the default — `useOpenKeys` will collapse siblings when opening a new branch.
- Don't render leaf rows for routes with `meta.hideMenu` or `REDIRECT_NAME`; filtering happens upstream of `items`.

## Dependencies
### Internal
- `/@/router/types`, `/@/router/constant`, `/@/logics/mitt/routeChange`, `/@/hooks/web/useDesign`, `/@/utils/is`, `/@/utils`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
