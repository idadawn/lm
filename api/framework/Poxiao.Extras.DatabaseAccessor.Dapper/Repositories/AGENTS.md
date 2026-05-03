<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Repositories

## Purpose
Dapper repository abstraction for application services. Wraps a scoped `IDbConnection` and surfaces the common Dapper operations (`Query`, `QueryAsync`, `Execute`, `ExecuteScalar`, multi-mapping, transactions) plus a `DynamicContext` for ad-hoc dynamic queries.

## Key Files
| File | Description |
|------|-------------|
| `IDapperRepository.cs` | Non-generic `IDapperRepository` contract (Query / Execute / Async variants, `Context`, `DynamicContext`). The file likely also declares the typed `IDapperRepository<TEntity>` partial. |
| `DapperRepository.cs` | Implementation backed by an injected `IDbConnection` and `IServiceProvider`; opens the connection lazily via the `Context` property. |

## For AI Agents

### Working in this directory
- Both the typed and non-typed repositories are registered scoped — share one connection per HTTP request.
- The repository is `partial`; new method clusters can be added in additional partial files (Insert/Update/Delete/etc.) to keep diffs reviewable.
- Always pass `IDbTransaction` through when callers supply one — do not silently swallow it.

### Common patterns
- The connection auto-opens on first access via `Context` getter; do not call `Open()` directly elsewhere.
- `DynamicContext` exists for plug-in providers that need a dynamic-language style (e.g., Oracle).

## Dependencies
### Internal
- Consumed via DI by application services (typically through `Poxiao.Apps` / `Poxiao.AI`).
### External
- `Dapper`, `Dapper.Contrib.Extensions`, `System.Data`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
