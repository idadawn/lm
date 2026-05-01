<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# User

## Purpose
Logged-in user runtime models — populated from the JWT/auth pipeline and consumed everywhere a service needs identity, permissions, data scope or online presence. The canonical "current user" shape lives here.

## Key Files
| File | Description |
|------|-------------|
| `UserInfoModel.cs` | The big "登录者信息" model — userId、userAccount、userName、headIcon、tenant info、roles、positions、organizations and (≈6 KB of) extra context fields. |
| `UserOnlineModel.cs` | Online session — `connectionId`、`userId`、`lastTime`、`lastLoginIp`、`lastLoginPlatForm`、`account`、`tenantId`、`token`、`onlineTicket`、`isMobileDevice`. |
| `UserDataScopeModel.cs` | 数据权限范围 model (data-scope by org/role/user). |
| `UserSystemModel.cs` | Per-user accessible 应用系统 entries. |
| `PositionInfoModel.cs` | 岗位 info attached to a user. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Models.User`.
- `UserInfoModel` is the contract shared by the JWT issuer, online cache, and every controller's `App.User` lookup — adding a field here ripples to every cache-of-user. Coordinate with `Manager/User/`.
- `UserOnlineModel.token` is stored verbatim in Redis; treat it like a credential.
- camelCase props consistently here; `[SuppressSniffer]` required.

### Common patterns
- All models are flat (no nested user objects beyond `PositionInfoModel`).
- Nullable strings only where the UI explicitly tolerates absence; otherwise default to empty string in producers.

## Dependencies
### Internal
- Wired by the auth/JWT pipeline (framework `Poxiao` core) and `Manager/User/`.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
