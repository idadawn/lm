<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# OContracts

## Purpose
Legacy-naming entity base classes for tables that use the historic `F_*` column convention (the bulk of feature modules in this codebase). `CLDEntityBase` is the workhorse: most LIMS entities derive from it.

## Key Files
| File | Description |
|------|-------------|
| `OEntityBase.cs` | `OEntityBase<TKey>` — `F_Id` (PK), `F_TenantId`. Implements `ITenantFilter`, `IOEntity<TKey>`. |
| `CLEntityBase.cs` | + `F_CREATORTIME` / `F_CREATORUSERID` / `F_LastModifyTime` / `F_LastModifyUserId`; provides `Creator()` and `LastModify()` virtual methods. |
| `OCEntityBase.cs` | Variant base with creator only. |
| `OCDEntityBase.cs` | Variant base with creator + delete. |
| `CLDEntityBase.cs` | The full LIMS base — `F_CREATORTIME`/`F_CREATORUSERID`/`F_ENABLEDMARK`/`F_LastModifyTime`/`F_LastModifyUserId`/`F_DeleteMark`/`F_DeleteTime`/`F_DeleteUserId`. Methods `Creator()`, `Create()`, `LastModify()`, `Delete()`. |
| `IOEntity.cs`, `IOCreatorTime.cs`, `IODeleteTime.cs` | Marker interfaces consumed by SqlSugar AOP filters and the unit-of-work fillers. |

## For AI Agents

### Working in this directory
- The four files `EntityBase.cs`, `ICreatorTime.cs`, `IDeleteTime.cs`, `IEntity.cs` in this folder are EXCLUDED from compilation by the csproj's `<Compile Remove>` block (they are duplicates of the modern variants). Don't add `using` references to them.
- All column names are case-sensitive on Oracle/MySQL — preserve the mixed casing exactly (`F_LastModifyTime` vs `F_CREATORTIME`).
- `CLDEntityBase.Creator()` writes a fallback CreatorUserId of all-zero GUID when no HTTP user context is present (e.g. DB seeders). Don't remove that branch.
- When a child entity needs to override one of these column mappings (e.g. legacy table with a different name), use `[SugarColumn(ColumnName="...")]` on the property and mark the override `virtual`/`override` per `.cursorrules`.

### Common patterns
- `Create()` only fills `CreatorTime` / `EnabledMark` / `Id` / `TenantId` if they are still null — reusing the same hook for inserts and migrations is intentional.
- `EnabledMark = 1` means active; `DeleteMark = 1` means soft-deleted.

## Dependencies
### Internal
- `Poxiao.Infrastructure.Const.ClaimConst`, `Poxiao.Infrastructure.Security.SnowflakeIdHelper`, `Poxiao.Extras.DatabaseAccessor.SqlSugar.Models.ITenantFilter`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
