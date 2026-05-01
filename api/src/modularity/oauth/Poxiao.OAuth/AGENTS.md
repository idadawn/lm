<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.OAuth

## Purpose
Implementation of the 身份认证模块. The single `OAuthService` (~76KB) is a `IDynamicApiController, ITransient` exposing `api/[controller]` endpoints: login (password + captcha + tenant resolution), refresh, logout, lock screen, third-party / social login callback, online ticket SSO, and the bootstrap `getCurrentUser` payload that returns user info, menu tree, permission matrix, and `SysConfigInfo` to the frontend.

## Key Files
| File | Description |
|------|-------------|
| `OAuthService.cs` | Whole-module service. Tag `OAuth`, route `api/[controller]`, Order 160. Coordinates `IUserService`, `IModuleService`, `IModuleButtonService`, `IModuleColumnService`, `IModuleDataAuthorizeSchemeService`, `IModuleFormService`, `ISysConfigService`, `IGeneralCaptcha`, `ISocialsUserService`. Reads `OauthOptions`, `ConnectionStringsOptions`, `TenantOptions` from app config. |
| `Poxiao.OAuth.csproj` | References `Poxiao.Common.Core`, `Poxiao.Message.Interfaces`, `Poxiao.Systems.Interfaces`; depends on `SkiaSharp` + `System.Drawing.Common` (captcha image rendering). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dto/` | Login / lock-screen inputs and the rich `CurrentUserOutput` bootstrap payload (see `Dto/AGENTS.md`). |
| `Model/` | Internal config models such as `SysConfigByOAuthModel` (see `Model/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `OAuthService` is intentionally a single large class. New auth endpoints should be added as methods on it, grouped under regions, rather than split into a sibling controller — matches the existing convention.
- Tenant + connection-string resolution happens here before user lookup (`_tenant`, `_connectionStrings`, `socialsOptions` on `LoginInput`). Be careful not to bypass tenant scoping.
- Lockout strategy is config-driven via `SysConfigByOAuthModel` (`lockType`, `passwordErrorsNumber`, `lockTime`) — read from `ISysConfigService`, not hardcoded.
- Captcha is mandatory when `enableVerificationCode` is on — every login path must call `_captchaHandler.Validate(...)` first.
- This service is `ITransient`; do not put per-request mutable state on instance fields.

### Common patterns
- All input DTOs validated via `[Required(ErrorMessage = "…")]` data annotations with Chinese messages.
- `[SuppressSniffer]` on every DTO/Model so DI scanning skips them.
- Cross-cut: `Poxiao.Logging.Attributes` for audit log, `Poxiao.EventBus` for login events, `Poxiao.UnifyResult` for response wrapping.

## Dependencies
### Internal
- `Poxiao.Systems.Interfaces` — modules, buttons, columns, forms, data authorize scheme, sys config, socials user.
- `Poxiao.Systems.Entitys` — `UserEntity`, permission models, `SysConfig` model namespace.
- `Poxiao.Message.Interfaces` — login notifications.
- `Poxiao.Common.Core` — shared base.
- `Poxiao.Infrastructure` — `IUserManager`, `IGeneralCaptcha`, `OauthOptions`, `ConnectionStringsOptions`, `TenantOptions`, `IHttpContextAccessor` accessors.
### External
- `Mapster`, `SqlSugar`, `Microsoft.AspNetCore.Authorization`, `Aop.Api` (Alipay AOP for social login), `SkiaSharp`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
