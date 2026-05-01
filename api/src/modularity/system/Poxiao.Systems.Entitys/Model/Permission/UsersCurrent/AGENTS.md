<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# UsersCurrent (Model)

## Purpose
当前登录用户授权聚合 Model。在请求处理早期由 `UsersCurrentService` 装配，包含菜单、按钮、列、数据权限以及主题/语言等个性化设置，缓存到 Redis 减少重复计算。

## Key Files
| File | Description |
|------|-------------|
| `UsersCurrentAuthorizeMoldel.cs` | 当前用户授权聚合（菜单 + 按钮 + 列 + 数据权限 + 角色 + 机构） |

## For AI Agents

### Working in this directory
- 文件名拼写为 `Moldel`（历史拼写），变更将影响序列化命名/反射，请勿改动。
- 字段增加时需要使现有缓存版本失效。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
