<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Role (Dto)

## Purpose
角色 DTO。角色是 RBAC 的核心，承载菜单/按钮/列/数据权限挂接。提供 CRUD、详情、列表、选择器、缓存列表。

## Key Files
| File | Description |
|------|-------------|
| `RoleCrInput.cs` / `RoleUpInput.cs` | 角色创建/更新（含权限集合） |
| `RoleInfoOutput.cs` | 详情（含已授权资源） |
| `RoleListOutput.cs` | 列表项 |
| `RoleListInput.cs` | 列表查询输入 |
| `RoleSelectorOutput.cs` | 选择器 |
| `RoleCacheListOutput.cs` | 缓存全量列表 |

## For AI Agents

### Working in this directory
- 角色编辑保存权限是高频操作，输入结构变化需同步 `AuthorizeService` 与 `Authorize/` DTO。
- 列表查询常带租户隔离，前端只需传过滤条件，不需传 TenantId。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
