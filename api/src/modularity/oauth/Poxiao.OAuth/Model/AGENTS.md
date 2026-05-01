<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Model

## Purpose
Internal config models used by `OAuthService` — not exposed directly over HTTP. Currently holds the typed view of the subset of `SysConfig` that affects the login pipeline (captcha toggle, lockout strategy, IP whitelist, token timeout, single-login mode).

## Key Files
| File | Description |
|------|-------------|
| `SysConfigByOAuthModel.cs` | Login-relevant `SysConfig` projection: `enableVerificationCode` (bool via `BoolJsonConverter`), `lockType` (`ErrorStrategy`: 1=lock account, 2=delay), `passwordErrorsNumber` (default 6), `lockTime` minutes (default 10), `whitelistSwitch` + `whiteListIp`, `tokenTimeout`, `singleLogin` (`LoginMethod`: 1=kick previous, 2=concurrent). |

## For AI Agents

### Working in this directory
- These are **internal** models — keep them out of the public Swagger surface. They are populated by `ISysConfigService` and consumed inside `OAuthService` only.
- `enableVerificationCode` and `whitelistSwitch` use `[JsonConverter(typeof(BoolJsonConverter))]` because the underlying SysConfig stores `"0"`/`"1"` strings — keep the converter on any new bool fields read from SysConfig.
- Defaults baked into the class (`passwordErrorsNumber = 6`, `lockTime = 10`) act as fallbacks when the SysConfig row omits the value — preserve them.
- Enum types `ErrorStrategy` and `LoginMethod` live in `Poxiao.Systems.Entitys.Enum`; reuse them instead of defining new ones.

### Common patterns
- `[SuppressSniffer]` to opt out of DI scanning.
- camelCase property names for JSON parity with frontend SysConfig editor.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- `Poxiao.JsonSerialization` — `BoolJsonConverter`.
- `Poxiao.Infrastructure.Enums` / `Poxiao.Systems.Entitys.Enum` — `ErrorStrategy`, `LoginMethod`.
### External
- `Newtonsoft.Json` — `JsonConverter` attribute.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
