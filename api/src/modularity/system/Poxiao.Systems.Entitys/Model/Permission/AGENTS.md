<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Permission (Model)

## Purpose
权限域复合 Model。聚合用户、机构、社交账号、当前用户授权等不直接持久化的视图对象，用于跨 Service 内部传递。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Authorize/` | 授权聚合（菜单+按钮+列+数据权限） (see `Authorize/AGENTS.md`) |
| `Organize/` | 机构属性 Model（机构 + 关联角色/岗位） (see `Organize/AGENTS.md`) |
| `SocialsUser/` | 第三方社交账号绑定信息 (see `SocialsUser/AGENTS.md`) |
| `User/` | SSO 用户信息 Model (see `User/AGENTS.md`) |
| `UsersCurrent/` | 当前登录用户授权聚合 Model (see `UsersCurrent/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 与 `Dto/Permission/*` 区别：Dto 是 API 边界，Model 是 Service 内部数据载体。
- 不要在此目录引用 `Poxiao.Systems.Interfaces`，仅依赖 Entity/Infrastructure。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
