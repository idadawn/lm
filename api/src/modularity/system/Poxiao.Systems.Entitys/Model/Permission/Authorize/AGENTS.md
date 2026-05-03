<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Authorize (Model)

## Purpose
授权聚合 Model。承载某用户/角色的菜单、按钮、列、数据权限合并结果，用于权限计算与缓存。

## Key Files
| File | Description |
|------|-------------|
| `AuthorizeModel.cs` | 授权数据组装结构（菜单 + 按钮 + 列 + 数据权限规则集合） |

## For AI Agents

### Working in this directory
- 该 Model 由 `AuthorizeService` 与 `UsersCurrentService` 在登录/取菜单时拼装并下发前端。
- 字段调整时务必同步前端权限指令（`v-perm`）与按钮渲染逻辑。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
