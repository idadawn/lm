<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Strongly-typed configuration bound from `Configurations/ConnectionStrings.json` and the `Tenant` section. Drives default connection selection, multi-tenant mode, and per-tenant connection construction (master/slave, isolation field, custom).

## Key Files
| File | Description |
|------|-------------|
| `ConnectionStringsOptions.cs` | `IConfigurableOptions<>` — list of `DbConnectionConfig` (extends SqlSugar `ConnectionConfig` with DBName/Host/Port/UserName/Password/DefaultConnection). `PostConfigure` defaults missing `ConfigId` to `"default"`. |
| `ConnectionConfigOptions.cs` | Per-tenant logical config — `ConfigId`, `IsCustom`, `IsolationField`, `IsMasterSlaveSeparation`, `ConfigList: List<DBConnectionConfig>` (IsMaster, ServiceName, dbType, connectionStr). |
| `TenantOptions.cs` | `MultiTenancy` toggle, `MultiTenancyType` (`SCHEMA` for DB isolation / `COLUMN` for field isolation), `MultiTenancyDBInterFace`. |

## For AI Agents

### Working in this directory
- Keys must match `appsettings`/`ConnectionStrings.json` exactly — renames are breaking config changes.
- `MultiTenancyType` is matched as a string with `Equals("SCHEMA")` in `SqlSugarRepository<T>`; treat the value as case-sensitive.
- Default `ConfigId` is `"default"` — preserve that fallback in `PostConfigure`.

### Common patterns
- `DbConnectionConfig` extends SqlSugar's own `ConnectionConfig` so it can be passed straight into `SqlSugarScope`.
- Mixed casing on properties (`dbType`, `connectionStr`) is legacy frontend contract; keep it.

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions.IConfigurableOptions`.
### External
- `SqlSugar.ConnectionConfig`, `SqlSugar.DbType`, `Microsoft.Extensions.Configuration`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
