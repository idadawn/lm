<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# User (Model)

## Purpose
用户域内部 Model。承载 SSO/单点登录场景下从外部身份源获取的用户信息（与系统本地 `UserEntity` 区分）。

## Key Files
| File | Description |
|------|-------------|
| `SSOUserInfoModel.cs` | SSO 用户信息：账户、姓名、邮箱、手机号、外部 ID 等 |

## For AI Agents

### Working in this directory
- 单点登录回调（OAuth/CAS 等）在 Service 中先组装 `SSOUserInfoModel`，再决定创建/绑定本地 `UserEntity`。
- 不存数据库；如要持久化字段请扩展 `UserEntity` 而非此 Model。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
