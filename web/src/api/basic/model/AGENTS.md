<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# model

## Purpose
TypeScript interface contracts for the OAuth / current-user payloads. Used by `basic/user.ts` callers and by `store/modules/user` when typing token + menu state.

## Key Files
| File | Description |
|------|-------------|
| `userModel.ts` | `LoginParams`, `LoginRequestParams`, `BackMenu` (recursive menu node from backend), `RouteItem`, `GetUserInfoModel` ({ menuList, permissionList, routerList, sysConfigInfo, userInfo }). |

## For AI Agents

### Working in this directory
- `BackMenu.children: Nullable<BackMenu[]>` — backend may return `null` rather than empty array; treat as such.
- `LoginParams` includes optional `jnpf_ticket` for the JNPF SSO flow; preserve when refactoring login.
- Sync with backend `Poxiao.System.Entitys` DTOs — particularly `UserMenu` / `MenuPermission` shapes.

### Common patterns
- Pure interfaces only — no runtime exports.

## Dependencies
### Internal
- `/#/store` (`UserInfo`, `SysConfigInfo`, `PermissionInfo`).
### External
- `vue-router` (`RouteMeta`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
