<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataParsing

## Purpose
Runtime parser that turns raw form data (`List<Dictionary<string, object>>`) into display-friendly dictionaries by resolving online-form controls — usersSelect, popupSelect, relationForm, popupTableSelect, address, etc. Used by every visualdev list/detail endpoint to substitute `userId--user` style references with `RealName/Account`, look up popupSelect rows, and rewrite child-table fields.

## Key Files
| File | Description |
|------|-------------|
| `ControlParsing.cs` | `ITransient`. Public methods: `GetParsDataList(...)`, `GetUsersSelectQueryWhere(...)`, `GenerateMultipleSelectionCriteriaForQuerying(...)`. Recursively walks `tablefield` children. |

## For AI Agents

### Working in this directory
- The class is large (≈460 lines) but cohesive — extend with new `case PoxiaoKeyConst.XXX:` branches inside `GetParsDataByList` rather than refactoring out.
- `vModel` keys for child tables use the `parent-child` separator. Preserve that contract (`item.Key + "-" + ctItem.Key`).
- Caches are keyed `CommonConst.CodeGenDynamic + "_" + interfaceId + "_" + tenantId` and live 10 minutes; user-component cache lives 5 minutes.

### Common patterns
- Falls back to `_formDataParsing.GetDynamicList(model)` (visualdev) on cache miss, then re-reads from cache so all paths see the same shape.
- For `RELATIONFORM`, executes inside `Scoped.Create(...)` to obtain `IRunService` from the request scope.

## Dependencies
### Internal
- `Poxiao.VisualDev.Engine`, `Poxiao.VisualDev.Interfaces`, `Poxiao.Systems.Entitys.Permission` (UserEntity, UserRelationEntity, OrganizeEntity, RoleEntity, PositionEntity, GroupEntity), `Poxiao.Infrastructure.Manager.ICacheManager`, `Poxiao.Infrastructure.Core.Manager.IUserManager`, `Poxiao.Systems.Interfaces.System.IDataInterfaceService`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
