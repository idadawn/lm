<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Permission (Dto)

## Purpose
权限/组织域全部 DTO。按子功能分目录：用户、机构、部门、岗位、角色、分组、用户关系、社交、当前用户等。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Authorize/` | 权限分配输入/输出 (see `Authorize/AGENTS.md`) |
| `Department/` | 部门 CRUD/选择器 (see `Department/AGENTS.md`) |
| `Group/` | 用户分组 (see `Group/AGENTS.md`) |
| `Organize/` | 机构 CRUD/树/成员 (see `Organize/AGENTS.md`) |
| `OrganizeAdministrator/` | 机构管理员 (see `OrganizeAdministrator/AGENTS.md`) |
| `Position/` | 岗位 (see `Position/AGENTS.md`) |
| `Role/` | 角色 (see `Role/AGENTS.md`) |
| `Socials/` | 第三方社交账号绑定 (see `Socials/AGENTS.md`) |
| `User/` | 用户 CRUD/导入导出/选择器 (see `User/AGENTS.md`) |
| `UserRelation/` | 用户关系（上下级） (see `UserRelation/AGENTS.md`) |
| `UsersCurrent/` | 当前登录用户辅助 (see `UsersCurrent/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 命名空间统一 `Poxiao.Systems.Entitys.Dto.{Authorize|Organize|...}`（注意命名空间不带 Permission 段）。
- DTO 字段在 `Mapper/PermissionMapper.cs` 中可能有自定义映射，调整字段名时同步检查。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
