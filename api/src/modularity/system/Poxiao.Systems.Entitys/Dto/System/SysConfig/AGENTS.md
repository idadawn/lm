<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SysConfig

## Purpose
DTOs for the 系统配置 (global settings) admin screen — covers system identity, login/security policy, OSS/SMTP/SMS providers, DingTalk and 企业微信 sync, login icons. The largest DTO bundle in the system module's entity layer.

## Key Files
| File | Description |
|------|-------------|
| `SysConfigCrInput.cs` | Master create/save input — 70+ fields (公司信息、单点登录、token超时、白名单、邮箱SMTP、短信厂商Ali/Tencent、钉钉/企业微信同步、登录图标、密码错误策略). |
| `SysConfigOutput.cs` | Read-side mirror returned to the settings UI. |
| `SysConfigUpInput.cs` | Lightweight per-section update input. |
| `SetAdminInput.cs` | 设置/重置 super-admin account. |
| `AdminUserOutput.cs` | Admin lookup result — `id`、`account`、`realName`. |

## For AI Agents

### Working in this directory
- `SysConfigCrInput.cs` is the canonical schema — when adding a new system-wide setting, **also** mirror it in `SysConfigOutput.cs`.
- Sensitive fields (`emailPassword`, `aliSecret`, `tencentSecretKey`, `qyhAgentSecret` ...) flow through these DTOs in clear text — service layer is responsible for masking on read and encrypting on write.
- Numeric flags use `int` 0/1 (启用/禁用), not `bool`, to stay consistent with frontend.
- Defaults set inline (e.g. `domain = "sms.tencentcloudapi.com"`, `lockTime = 10`) — preserve them when refactoring.

### Common patterns
- Region-grouped sections via `#region 短信` etc. for readability.
- `[SuppressSniffer]` on all classes; camelCase props.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Backs `base_sys_config` entity in `Poxiao.Systems` module.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
