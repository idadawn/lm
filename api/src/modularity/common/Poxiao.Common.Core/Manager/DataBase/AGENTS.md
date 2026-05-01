<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataBase

## Purpose
Cross-database SqlSugar wrapper. The visualdev / data-interface modules need to query arbitrary external databases (the `DbLinkEntity` rows configured by an admin), so this manager centralises connection switching, table introspection, paged dynamic queries, sync, and DDL.

## Key Files
| File | Description |
|------|-------------|
| `IDataBaseManager.cs` | Big interface (~30 methods). Categories: tenant connection (`GetTenantDbLink`, `GetTenantSqlSugarClient`, `ChangeDataBase`), DDL (`Create`, `Update`, `Delete`, `AddTableColumn`), introspection (`IsAnyTable`, `IsAnyColumn`, `GetFieldList`, `GetDBTableList`, `GetTableInfos`), data ops (`ExecuteSql`, `ExecuteReturnIdentityAsync`, `GetData`, `GetInterFaceData`, `GetDataTablePage`, `UseStoredProcedure`), sync (`SyncData`, `SyncTable`), type conversion (`ToDbType`, `ToConnectionString`, `ViewDataTypeConversion`). |
| `DataBaseManager.cs` | Implementation; backs the interface. |

## For AI Agents

### Working in this directory
- Every method takes a `DbLinkEntity link` so it can target either the current tenant DB or an external DB-link. New methods should follow that contract.
- Multi-database compatibility (SQL Server / MySQL / Oracle) is non-negotiable — any new SQL must be expressed via SqlSugar's `IConditionalModel` / `Queryable<T>` rather than raw vendor-specific syntax.
- `GetInterFaceData(...)` returns `PageResult<Dictionary<string, object>>`; preserve that signature — the visualdev list views serialise it directly.

### Common patterns
- DDL goes through `sqlSugarScope.CodeFirst.InitTables(...)` / `EntityMaintenance.AddColumn(...)` rather than ALTER TABLE strings.
- View vs table is distinguished by the `isView` parameter on `GetTableInfos`.

## Dependencies
### Internal
- `Poxiao.Systems.Entitys.System.DbLinkEntity`, `Poxiao.Systems.Entitys.Dto.Database`, `Poxiao.VisualDev.Entitys`, `Poxiao.Infrastructure.Dtos.DataBase`, `Poxiao.Infrastructure.Models.VisualDev.MainBeltViceQueryModel`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
