<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Permission (Interfaces)

## Purpose
权限/组织域服务接口。声明用户、机构、部门、角色、岗位、分组、社交账号、用户关系、当前用户等核心 RBAC 服务契约。

## Key Files
| File | Description |
|------|-------------|
| `IUsersService.cs` | 用户查询（按 Id/账户/登录信息）、CRUD 等契约 |
| `IOrganizeService.cs` | 机构树、CRUD、选择器 |
| `IDepartmentService.cs` | 部门管理 |
| `IRoleService.cs` | 角色管理与角色-权限挂接 |
| `IPositionService.cs` | 岗位管理 |
| `IUserGroupService.cs` | 用户分组 |
| `IAuthorizeService.cs` | 权限分配/查询聚合 |
| `IUsersCurrentService.cs` | 当前用户上下文（菜单、主题、签名等） |
| `IUserRelationService.cs` | 用户关系（上下级） |
| `IOrganizeAdministratorService.cs` | 机构管理员 |
| `ISocialsUserService.cs` | 第三方账号绑定 |

## For AI Agents

### Working in this directory
- 跨模块需要访问"当前登录用户"或"用户基础信息"时，请注入 `IUsersCurrentService` / `IUsersService`，不要直接依赖 `UserEntity` 仓储。
- 接口返回类型使用 Entity 或 Dto，遵循已有约定（多数返回 Entity，让调用方按需 Adapt）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
