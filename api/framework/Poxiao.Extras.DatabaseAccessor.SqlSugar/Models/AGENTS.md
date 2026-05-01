<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Models

## Purpose
Shared models used by the SqlSugar tenant pipeline. Defines the column-isolation marker interface, the per-request tenant cache shape, and the persisted tenant-link record (database connection metadata for one tenant).

## Key Files
| File | Description |
|------|-------------|
| `ITenantFilter.cs` | `TenantId` (mapped to column `F_TenantId`) — entities implementing this opt into column-level isolation filters. |
| `GlobalTenantCacheModel.cs` | Cached `TenantId`, `SingleLogin` mode, and `ConnectionConfigOptions`; stored under `poxiao:globaltenant` in `ICacheManager`. |
| `TenantLinkModel.cs` | Persisted DB-link row (host, port, userName, password, dbType, configType 0/1 主/从, oracleParam, custom `connectionStr`, schema, tableSpace). |

## For AI Agents

### Working in this directory
- Property casing on `TenantLinkModel` is **camelCase** (legacy frontend contract) — preserve it; the LIMS web UI binds these names directly.
- `[SuppressSniffer]` on `GlobalTenantCacheModel` opts out of dependency-scan registration; keep it on cache POCOs.
- `ITenantFilter.TenantId` column name is locked to `F_TenantId` to align with `OEntityBase` — do not rename.

### Common patterns
- `connectionStr` allows full-string override; otherwise host/port/userName/password are formatted into the dialect's connection template.
- `configType` enumerates 0=主 (master) / 1=从 (slave) for read-write splitting.

## Dependencies
### Internal
- `ConnectionConfigOptions` from `../Options`; `Poxiao.DependencyInjection.SuppressSniffer`.
### External
- `SqlSugar.SugarColumn`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
