<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Socials (Dto)

## Purpose
第三方社交账号绑定 DTO。当前仅一份输出，用于在用户管理页展示某用户已绑定的社交账号列表。

## Key Files
| File | Description |
|------|-------------|
| `SocialsUserListOutput.cs` | 已绑定的社交账号列表（平台、Open/Union Id、绑定时间） |

## For AI Agents

### Working in this directory
- 解绑/绑定动作复用 `UsersService` / `SocialsUserService` 现有接口，不在此处再加 Input DTO。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
