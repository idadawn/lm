<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Internal helpers used by the Dapper accessor. Currently maps a provider name (e.g., `Microsoft.Data.SqlClient`) to its ADO.NET `IDbConnection` implementation by scanning the loaded assemblies and caching the result.

## Key Files
| File | Description |
|------|-------------|
| `SqlProvider.cs` | Static class with provider-name constants (`SqlServer`, `Sqlite`, `MySql`, `Npgsql`, `Oracle`, `Firebird`) and `GetDbConnectionType(string)` reflection lookup with a `ConcurrentDictionary<string,Type>` cache. |

## For AI Agents

### Working in this directory
- Adding a new provider = add a constant + ensure the corresponding NuGet package is referenced by the host application (this assembly does **not** ship those drivers).
- Keep the cache lookup thread-safe; `ConcurrentDictionary` is intentional.
- Anything in this folder is `internal`-grade; do not expose new APIs without versioning the package.

### Common patterns
- Lazy reflection: assemblies are only inspected on first use of a given provider key.

## Dependencies
### External
- BCL only; reflection over loaded ADO.NET provider assemblies.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
