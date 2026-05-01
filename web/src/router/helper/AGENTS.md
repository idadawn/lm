<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# helper

## Purpose
Pure transform utilities for menu trees and route records. Translates between backend `BackMenu` flat lists, the in-memory `Menu[]` tree, and vue-router `RouteRecordRaw[]` — including dynamic `() => import()` component lookup and multi-level flattening.

## Key Files
| File | Description |
|------|-------------|
| `menuHelper.ts` | `transformMenuModule`, `joinParentPath`, `getAllParentPath`, `transformRouteToMenu` — joins child paths, finds breadcrumb chain via `treeHelper.findPath`. |
| `routeHelper.ts` | `transformObjToRoute` (resolves component strings to async-imports), `flatMultiLevelRoutes` (collapses 3+ level routes for keep-alive), and dynamic page-component map built from `import.meta.glob('/@/views/**/*.{vue,tsx}')`. |

## For AI Agents

### Working in this directory
- Backend stores route component as a string (`urlAddress`, `propertyJson`); `routeHelper.ts` resolves it via the glob map — new view directories must match the glob pattern.
- `flatMultiLevelRoutes` is required because keep-alive only caches first-level children; do not bypass.
- `LAYOUT` / `EXCEPTION_COMPONENT` placeholders come from `/@/router/constant`.

### Common patterns
- Recursion over menu trees uses `lodash-es` `cloneDeep` to avoid mutating reactive data.
- URL-vs-path detection via `/@/utils/is.isUrl`.

## Dependencies
### Internal
- `/@/utils/helper/treeHelper`, `/@/utils/is`, `/@/router/constant`, `/@/router/types`.
### External
- `lodash-es`, `vue-router`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
