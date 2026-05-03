<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Model

## Purpose
非数据库实体的视图/聚合 Model，用于 Service 内部组装数据或对外返回复合结构（菜单功能聚合、SSO 用户、动态表结构等）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Permission/` | 权限聚合 Model：用户、机构、社交、当前用户授权等 (see `Permission/AGENTS.md`) |
| `System/` | 系统聚合 Model：菜单功能、动态库表、数据接口请求体、打印数据等 (see `System/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 这里的类不写 `[SugarTable]`，仅做内存对象，多用 `[SuppressSniffer]` 防止被动态 API 探测。
- 与 Entity 的差异：Model 通常包含派生字段、跨表组合、前端友好命名（camelCase）。
- 命名后缀统一 `Model`。

### Common patterns
- 继承 `FunctionalBase` 等基类做菜单/按钮/列/表单的通用属性复用。

## Dependencies
### Internal
- 同项目 Entity（部分 Model 引用 Entity 作子节点）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
