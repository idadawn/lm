<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Permission

## Purpose
权限/组织域 Service 实现。覆盖用户、机构、部门、角色、岗位、分组、社交登录绑定、用户关系（上下级）以及当前登录用户上下文等核心 RBAC 能力。

## Key Files
| File | Description |
|------|-------------|
| `UsersService.cs` | 用户 CRUD、登录、密码、导入/导出、当前登录用户辅助；路由 `api/permission/Users` |
| `OrganizeService.cs` | 组织树、机构 CRUD 与树形选择器 |
| `DepartmentService.cs` | 部门管理（机构子集） |
| `RoleService.cs` | 角色管理与角色-菜单/按钮/数据权限挂接 |
| `PositionService.cs` | 岗位管理 |
| `GroupService.cs` | 用户分组 |
| `AuthorizeService.cs` | 权限分配与查询（菜单/按钮/列权限/数据权限聚合） |
| `UsersCurrentService.cs` | 当前用户信息、主题、语言、签名图片、子级、密码修改 |
| `UserRelationService.cs` | 用户关系（上下级、关注） |
| `OrganizeAdministratorService.cs` | 机构管理员配置 |
| `SocialsUserService.cs` | 第三方账号绑定（钉钉/企业微信等） |

## For AI Agents

### Working in this directory
- Service 必须实现 `Poxiao.Systems.Interfaces.Permission.IXxxService`；新建用户/角色相关接口时同步更新 Mapper 中的映射规则。
- 用户密码字段加密走 `Poxiao.DataEncryption`，不要自己实现哈希。
- `Order` 段在 160-180 区间为 Permission 子域常用。

### Common patterns
- 注入多个仓储（如 `UsersService` 同时持有 User/Role/Organize 等仓储以做联合查询）。
- 大量使用 Mapster `.Adapt<TOutput>()` 与 SqlSugar 表达式 `Expression<Func<T, bool>>` 动态条件。
- 树形输出（机构、部门）用递归 + `OrganizeTreeOutput`/`DictionaryDataTreeOutput` 模式。

## Dependencies
### Internal
- `Poxiao.Systems.Interfaces.Permission`、`Poxiao.Systems.Entitys.Permission`、`...Dto.Permission.*`
- `infrastructure/Poxiao.Extras.CollectiveOAuth`（社交登录）
- `Poxiao.Infrastructure.Models.User`（UserInfoModel）

### External
- SqlSugar、Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
