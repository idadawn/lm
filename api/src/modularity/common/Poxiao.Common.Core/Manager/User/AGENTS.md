<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# User

## Purpose
Request-scoped façade over the JWT user context — exposes `UserId`, `TenantId`, `Account`, `RealName`, `Roles`, `IsAdministrator`, the user's data scope, and computed sets (current org + sub-orgs, user + subordinates). Also produces SqlSugar `IConditionalModel` lists for module-level data permission filtering.

## Key Files
| File | Description |
|------|-------------|
| `IUserManager.cs` | Properties: `UserId`, `Roles`, `TenantId`, `TenantDbName`, `Account`, `RealName`, `ToKen`, `IsAdministrator`, `Subordinates`, `CurrentUserAndSubordinates`, `CurrentOrganizationAndSubOrganizations`, `CurrentUserSubOrganization`, `DataScope`, `UserOrigin` (pc/app), `User` (UserEntity), `CurrentTenantInformation`. Methods: `GetUserInfo()`, `GetConditionAsync<T>(moduleId, primaryKey, isDataPermissions, tableNumber)`, `GetDataConditionAsync<T>(...)`, `GetCondition<T>(primaryKey, moduleId, isDataPermissions)`, `GetCodeGenAuthorizeModuleResource<T>(...)`, `GetRoleNameByIds(ids)`, `GetUserOrgRoleIds(roleIds, organizeId)`, `GetUserName(userId, isAccount)` + async variant, `GetAdminUserId()`. |
| `UserManager.cs` | Implementation; reads JWT claims via `App.User`, caches role/data-scope lookups in Redis. |

## For AI Agents

### Working in this directory
- `GetConditionAsync<T>` defaults to `primaryKey="F_Id"` — matches the legacy `OEntityBase` PK. Pass an explicit primary key for tables that derive from the modern `EntityBase` hierarchy.
- `tableNumber` is the join-table alias used inside `IConditionalModel` rendering; populate it when the query is part of a multi-table join.
- `UserOrigin` resolves from the `userOrigin` request header (`pc` / `app`); downstream parser code (`ControlParsing`) branches on this.

### Common patterns
- Returns `List<IConditionalModel>` so callers can splice into `Queryable<T>().Where(conditions)` directly.
- Admin/super-user short-circuits return an empty condition list.

## Dependencies
### Internal
- `Poxiao.Systems.Entitys.Permission.UserEntity`, `Poxiao.Infrastructure.Models.User`, `Poxiao.Infrastructure.Models.Authorize.CodeGenAuthorizeModuleResourceModel`, `Poxiao.Infrastructure.Models.User.UserDataScopeModel`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
