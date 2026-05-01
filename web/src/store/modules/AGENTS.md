<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# modules

## Purpose
Domain Pinia stores. Each file owns a slice of app state — user/auth, dynamic permissions/menu tree, project config, multi-tab tabs, locale, lock screen, error log, charts data cache, organisation tree, and code-generator transient state.

## Key Files
| File | Description |
|------|-------------|
| `app.ts` | Project config, dark mode, page loading, header/menu/transition settings; persists `PROJ_CFG_KEY`. |
| `user.ts` | Token, `userInfo`, `permissionList`, `backMenuList`, `backRouterList`, `sessionTimeout`; orchestrates login, `getUserInfoAction`, logout, and afterLogin hooks. |
| `permission.ts` | `menuList`, `isDynamicAddedRoute`; `buildRoutesAction()` calls backend then transforms via `routeHelper.transformObjToRoute` + `flatMultiLevelRoutes`. |
| `multipleTab.ts` | Open tab list, drag-end index, cache set; persisted; consumed by the multi-tab bar. |
| `permission.ts`, `base.ts`, `organize.ts` | Bootstrapped on login (orgs/dicts/system base data). |
| `errorLog.ts` | Holds error log entries from `logics/error-handle`. |
| `locale.ts` | Active locale + i18n resource toggling. |
| `lock.ts` | Lock-screen state (timestamp + password hash). |
| `chart.ts` | Chart data caches for dashboards. |
| `generator.ts` | Transient state for code-generator wizard. |

## For AI Agents

### Working in this directory
- `defineStore({ id, state, getters, actions, store })` — always pass the shared `store` import so `*StoreWithOut` works.
- Persistence uses `/@/utils/cache/persistent` keyed by `cacheEnum` constants (e.g. `TOKEN_KEY`, `USER_INFO_KEY`, `MULTIPLE_TABS_KEY`).
- Login/logout is orchestrated from `user.ts` — do not trigger `router.push` from here without going through `usePermissionStore.buildRoutesAction()`.
- 注意: this project's backend is a .NET / SqlSugar modular monolith — `BackMenu` shape comes from `/api/oauth/CurrentUser`.

### Common patterns
- Async actions return `Promise<...>`, throw on auth failure; UI catches at the call site.
- Avoid storing large arrays from API responses unless they're keyed caches; prefer view-level state.

## Dependencies
### Internal
- `/@/api/basic`, `/@/api/permission`, `/@/router`, `/@/utils/cache/persistent`, `/@/utils/auth`, `/@/enums/cacheEnum`, `/@/enums/pageEnum`.
### External
- `pinia`, `vue`, `vue-router`, `lodash-es`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
