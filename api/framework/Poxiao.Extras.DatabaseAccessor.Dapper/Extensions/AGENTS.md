<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
DI registration entry point for the Dapper accessor. Resolves an `IDbConnection` for the configured provider, registers the typed and non-typed `IDapperRepository`, and exposes a hook for additional Dapper configuration.

## Key Files
| File | Description |
|------|-------------|
| `DapperServiceCollectionExtensions.cs` | `AddDapper(IServiceCollection, connectionString, sqlProvider, configure?)` — scoped `IDbConnection`, `IDapperRepository`, `IDapperRepository<>`. |

## For AI Agents

### Working in this directory
- Extension namespace is `Microsoft.Extensions.DependencyInjection` to keep call sites clean.
- The connection is `AddScoped` — never promote it to singleton.
- The optional `configure` callback is the right place to register Dapper type handlers, custom mappers, or column mappings.

### Common patterns
- Provider key passed as a string (matches `SqlProvider.SqlServer`, etc.); `SqlProvider.GetDbConnectionType` does the actual ADO.NET type lookup via reflection.

## Dependencies
### Internal
- `Dapper.SqlProvider`, `Dapper.IDapperRepository<T>`, `Dapper.DapperRepository<T>`.
### External
- `Dapper`, `Microsoft.Extensions.DependencyInjection.Abstractions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
