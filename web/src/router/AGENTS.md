<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# router

## Purpose
Vue Router 4 wiring for the LIMS web shell. Builds the router instance with static `basicRoutes`, registers white-listed names, and exposes `setupRouter(app)` plus `resetRouter()` used at logout. Dynamic backend-driven routes are appended later by `permissionGuard` from `permissionStore.buildRoutesAction`.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Creates the `createWebHistory` router with `basicRoutes`, exports `router`, `resetRouter`, `setupRouter`. |
| `constant.ts` | LAYOUT / EXCEPTION_COMPONENT placeholders, route names (`PAGE_NOT_FOUND_NAME`, `REDIRECT_NAME`). |
| `types.ts` | `AppRouteRecordRaw`, `Menu`, `MenuModule`, `AppRouteModule` typings. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `guard/` | beforeEach guards: permission, state, paramMenu (see `guard/AGENTS.md`). |
| `helper/` | Tree/menu/route transform utilities (see `helper/AGENTS.md`). |
| `menus/` | Async menu accessors built from permission store (see `menus/AGENTS.md`). |
| `routes/` | Static `basicRoutes` plus per-feature route modules (see `routes/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Static routes live in `routes/`; dynamic ones come from backend `BackMenu[]` via `permissionStore`. Do not hard-code feature pages in `basicRoutes` unless they need to render before login.
- `WHITE_NAME_LIST` is built recursively from `basicRoutes` — anything outside is removed by `resetRouter()`.

### Common patterns
- `/@/router/types` is the canonical source for route shape; backend-driven menus are converted via `helper/routeHelper.ts`.

## Dependencies
### Internal
- `/@/store/modules/permission`, `/@/store/modules/user`.
### External
- `vue-router`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
