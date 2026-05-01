<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# oauth

## Purpose
身份认证 (OAuth / login / SSO) module group. Owns the entire authentication surface of the Laboratory Data Analysis System: account login, captcha gating, lock screen, third-party / social callback, single-sign-on tickets, lockout strategy, and the "current user" payload (menus + permissions + system config) consumed by the Vue 3 frontend on app boot.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.OAuth/` | Single implementation project — `OAuthService` plus `Dto/` and `Model/` (see `Poxiao.OAuth/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Like `codegen`, this is a **single-project module group** — there are no separate `*.Entitys` / `*.Interfaces` projects. Login DTOs live next to the service in `Poxiao.OAuth/Dto/`.
- Authentication contracts (`UserInfoModel`, `OauthOptions`, `ISocialsUserService`) come from `Poxiao.Systems.Interfaces` and `Poxiao.Infrastructure` — do not redefine them here.
- All security-critical changes here must preserve captcha + lockout + tenant resolution semantics; review `OAuthService.Login` carefully before refactoring.

## Dependencies
### Internal
- `Poxiao.Systems.Interfaces` — module/menu/permission/sysconfig services.
- `Poxiao.Message.Interfaces` — login event/notification messages.
- `Poxiao.Common.Core` — shared infrastructure.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
