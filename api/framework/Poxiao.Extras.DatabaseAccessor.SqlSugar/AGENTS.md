<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extras.DatabaseAccessor.SqlSugar

## Purpose
Primary database accessor for the LIMS backend. Adapts SqlSugar (`SqlSugarCore` 5.1.4) into the Poxiao DI/config story and implements **multi-tenant** routing — supports column-level isolation (`F_TenantId`) and schema/database-level isolation by switching the active `SqlSugarScope` connection per HTTP request.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `AddSqlSugar`, paged-query helpers, JSON SQL functions, tenant-link helpers (see `Extensions/AGENTS.md`). |
| `Internal/` | `SqlSugarPagedList<T>` + `Pagination` DTOs (see `Internal/AGENTS.md`). |
| `Models/` | `ITenantFilter`, `GlobalTenantCacheModel`, `TenantLinkModel` (see `Models/AGENTS.md`). |
| `Options/` | `ConnectionStringsOptions`, `ConnectionConfigOptions`, `TenantOptions` (see `Options/AGENTS.md`). |
| `Repositories/` | `ISqlSugarRepository<T>` + tenant-aware implementation (see `Repositories/AGENTS.md`). |

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Extras.DatabaseAccessor.SqlSugar.csproj` | net10.0 library; references `Poxiao` and `SqlSugarCore` 5.1.4.211. |

## For AI Agents

### Working in this directory
- Entities follow the `CLDEntityBase` rules in repo `.cursorrules` — legacy field names use `[SugarColumn(ColumnName="...")]` overrides; missing fields use `[SugarColumn(IsIgnore=true)]`.
- Multi-tenant repos read `Tenant` and `ConnectionStrings` config sections via `App.GetConfig<>` — do not bypass the repository to query directly.
- Keep multi-database compatibility (SQL Server / MySQL / Oracle) when adding query helpers.

### Common patterns
- `SqlSugarScope` registered as a singleton (it's documented as thread-safe by SqlSugar).
- Tenant switch happens inside `SqlSugarRepository<T>` ctor by reading the `TenantId` claim and a cached `GlobalTenantCacheModel` from `ICacheManager`.

## Dependencies
### Internal
- `Poxiao.csproj` (App / Options / Cache / FriendlyException / Logging).
### External
- `SqlSugarCore` 5.1.4.211, `Microsoft.AspNetCore.App` shared framework.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
