<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dto

## Purpose
系统模块全部 API 输入/输出 DTO 集中目录。按业务域分 Permission/ 与 System/ 两大子目录，进一步按功能再分子目录（一个功能 = 一个目录，含 Cr/Up/Info/List/Selector/Query 等多份 DTO）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Permission/` | 权限/组织域 DTO（用户、机构、角色、岗位等） (see `Permission/AGENTS.md`) |
| `System/` | 系统级 DTO（菜单、字典、日志、调度等） (see `System/AGENTS.md`) |

## For AI Agents

### Working in this directory
- DTO 类必须 `[SuppressSniffer]` 防止动态 API 误生成路由。
- 字段命名以 camelCase 居多，前端直接消费；对于历史 PascalCase 字段保持现状。
- 命名约定：`*CrInput`（Create）、`*UpInput`（Update）、`*ListOutput`（列表项）、`*InfoOutput`（详情）、`*SelectorOutput`（选择器/下拉）、`*Query`/`*ListQuery`（查询条件）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
