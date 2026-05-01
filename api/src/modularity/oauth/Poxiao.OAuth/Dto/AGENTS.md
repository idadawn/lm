<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dto

## Purpose
Request/response shapes for the OAuth module's HTTP surface — login + lock-screen inputs and the rich `CurrentUserOutput` bootstrap payload that the Vue frontend fetches once on login to render the whole authenticated shell (user info, menu tree, permission matrix, system branding).

## Key Files
| File | Description |
|------|-------------|
| `LoginInput.cs` | Login request: `account`, `password` (md5), `code`+`timestamp` (captcha), `origin`, third-party `socialsOptions` (`SqlSugar.ConnectionConfigOptions`), `isSocialsLoginCallBack`, `poxiaoTicket` (auto-bind cache key), `onlineTicket` (SSO ticket). |
| `LockScreenInput.cs` | Re-auth payload for the lock-screen unlock flow: `account` + `password` only. |
| `CurrentUserOutput.cs` | Bootstrap payload — `userInfo` (UserInfoModel), `menuList`, `permissionList` (per-module `column` / `button` / `form` / `resource` arrays), `sysConfigInfo` (title/logo/branding), `routerList` (full menu tree as `UserAllMenu`). Defines several internal model classes inline. |
| `CurrentUserModelOutput.cs` | Flat per-user authorization arrays — `moduleList`, `buttonList`, `columnList`, `resourceList` (data-authorize scheme), `formList`. Used by lighter "what can I do" lookups. |

## For AI Agents

### Working in this directory
- Property names are camelCase to match the frontend contract (`userInfo`, `menuList`, `permissionList`, `sysConfigInfo`, `routerList`, `enCode`). **Do not** rename.
- `[SuppressSniffer]` is required on every DTO so DI scanning skips them.
- `LoginInput` carries optional `SqlSugar.ConnectionConfigOptions` for cross-tenant social callback — preserve nullability and don't log it (contains DB credentials).
- Validation messages are Chinese (`"用户名不能为空"`, `"密码不能为空"`); keep the same phrasing for new required fields.
- `CurrentUserOutput.cs` defines several auxiliary classes (`PermissionModel`, `FunctionalAuthorizeBase`, `FunctionalColumnAuthorizeModel`, `FunctionalButtonAuthorizeModel`, `FunctionalFormAuthorizeModel`, `FunctionalResourceAuthorizeModel`, `SysConfigInfo`, `UserAllMenu`) in the same file — keep them co-located rather than splitting.

### Common patterns
- `[Required]` data annotations with Chinese error messages on inputs.
- XML `<example>` tags on `LoginInput` properties (`13459475357`, md5 example) feed Swagger UI.
- Outputs reuse `UserInfoModel` from `Poxiao.Infrastructure.Models.User` and `ModuleNodeOutput` from `Poxiao.Systems.Entitys.Dto.Module`.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- `Poxiao.Infrastructure.Models.User` — `UserInfoModel`.
- `Poxiao.Infrastructure.Security` — security base types.
- `Poxiao.Systems.Entitys.Dto.Module*` — module/button/column/form/data-authorize DTOs reused in outputs.
### External
- `System.ComponentModel.DataAnnotations` — `[Required]`.
- `SqlSugar` — `ConnectionConfigOptions` on `LoginInput.socialsOptions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
