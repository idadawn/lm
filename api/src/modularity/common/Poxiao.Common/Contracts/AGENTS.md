<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Contracts

## Purpose
SqlSugar entity base classes and the marker interfaces (`IEntity`, `ICreatorTime`, `IDeleteTime`) they implement. Two parallel hierarchies coexist intentionally — see CRITICAL note below. New tables should derive from one of them depending on whether they have legacy `F_*` column names or new snake_case ones.

## Key Files
| File | Description |
|------|-------------|
| `EntityBase.cs` | Modern base — `id` (PK) + `tenant_id`. Implements `ITenantFilter`, `IOEntity<TKey>`. |
| `CDEntityBase.cs` | Modern + soft-delete (`created_time`, `created_userid`, `delete_time`, `delete_userid`, `is_deleted`). |
| `CUDEntityBase.cs` | Modern + soft-delete + last-modified (`last_modified_time`, `last_modified_userid`). |
| `CEntityBase.cs` | Modern + creator only. |
| `CUEntityBase.cs` | Modern + creator + last modified. |
| `IEntity.cs` / `ICreatorTime.cs` / `IDeleteTime.cs` | Marker interfaces consumed by SqlSugar AOP for auto-fill. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `OContracts/` | Legacy-naming bases (`F_Id`, `F_TenantId`, `F_CREATORTIME`, …). All current LIMS feature tables derive from `CLDEntityBase` here (see `OContracts/AGENTS.md`) |

## For AI Agents

### Working in this directory
- CRITICAL: Read `/data/project/lm/.cursorrules` before adding/editing entities. Field naming is split:
  - `EntityBase`/`CDEntityBase`/`CUDEntityBase` use snake_case (`id`, `created_time`, …).
  - `OEntityBase`/`CLEntityBase`/`CLDEntityBase` use legacy `F_Id`, `F_CREATORTIME`, mixed-case `F_LastModifyTime`/`F_LastModifyUserId`/`F_DeleteMark`/`F_DeleteTime`/`F_DeleteUserId`, all-caps `F_CREATORTIME`/`F_CREATORUSERID`/`F_ENABLEDMARK`.
- The csproj-level `<Compile Remove>` strips four duplicates from `OContracts/` (`EntityBase.cs`, `ICreatorTime.cs`, `IDeleteTime.cs`, `IEntity.cs`); don't reintroduce them.
- `Create()` uses `SnowflakeIdHelper.NextId()` for primary keys and pulls `userId` / `tenantId` from `App.User?.FindFirst(ClaimConst.*)`. When there is no HTTP context (DB init), `CLDEntityBase.Creator()` falls back to `00000000-0000-0000-0000-000000000000`.

### Common patterns
- Every column carries `[SugarColumn(ColumnName=..., ColumnDescription=..., IsNullable=...)]`.
- `EnabledMark` defaults to `1` on create (启用); `DeleteMark` is set to `1` on soft-delete.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
