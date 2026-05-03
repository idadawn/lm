<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SimpleMenu

## Purpose
Lightweight sidebar menu used by the mix-sider/sider layout. Unlike `ant-design-vue`'s `Menu`, this implementation supports custom collapse animation, route-driven active state, and accordion behaviour with explicit context — designed to perform well with deeply nested route trees.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `SimpleMenu` and `SimpleMenuTag`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Top-level menu, tag, sub-menu wrappers, `useOpenKeys` hook, LESS (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Active key tracking is route-driven via `listenerRouteChange` + `REDIRECT_NAME` (skip the redirect placeholder route).
- `accordion` defaults to true — only one open submenu at a time.
- External URLs detected via `isUrl` open through `openWindow`, not `router.push`.

### Common patterns
- Vue 3 `provide`/`inject` rooted in `Menu.vue` (see `src/components/useSimpleMenuContext.ts`).

## Dependencies
### Internal
- `/@/router/types` (`Menu`), `/@/router/constant` (`REDIRECT_NAME`), `/@/logics/mitt/routeChange`, `/@/hooks/web/useDesign`, `/@/utils/is`.
### External
- `vue-router`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
