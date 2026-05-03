<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extras.DatabaseAccessor.Dapper

## Purpose
Dapper-based database accessor adapter for the Poxiao framework. Provides a pluggable `IDapperRepository` (typed and non-typed) plus DI registration that resolves a scoped `IDbConnection` from a configurable provider name (SqlServer / Sqlite / MySql / Npgsql / Oracle / Firebird).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `AddDapper` service-collection extension (see `Extensions/AGENTS.md`). |
| `Internal/` | `SqlProvider` provider-name → `IDbConnection` type mapping (see `Internal/AGENTS.md`). |
| `Repositories/` | Generic and non-generic Dapper repositories (see `Repositories/AGENTS.md`). |

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Extras.DatabaseAccessor.Dapper.csproj` | net10.0 packable library; pulls in Dapper. |

## For AI Agents

### Working in this directory
- This is the *secondary* DB accessor in the project — most modules use `Poxiao.Extras.DatabaseAccessor.SqlSugar`. Reach for Dapper only when raw SQL ergonomics are required.
- Never hard-code provider strings; use the constants in `Internal/SqlProvider.cs` (`SqlServer`, `Sqlite`, `MySql`, `Npgsql`, `Oracle`, `Firebird`).
- Multi-database compatibility (MySQL / SQL Server / Oracle) is project policy — keep raw SQL portable or branch on the active provider.

### Common patterns
- Single scoped `IDbConnection` per request; the connection is opened lazily by `DapperRepository.Context`.
- `Activator.CreateInstance(dbConnectionType, connectionString)` reflective construction lets new providers be added with no code change beyond registering an assembly.

## Dependencies
### External
- Dapper, Dapper.Contrib, ADO.NET providers loaded by name from `SqlProvider`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
