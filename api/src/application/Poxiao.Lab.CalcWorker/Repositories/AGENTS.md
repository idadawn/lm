<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Repositories

## Purpose
Worker-only repository implementations. Provides a minimal `ISqlSugarRepository<T>` so that calc / judgement code originally written against the framework's `SqlSugarRepository` can run inside the standalone worker without pulling in `App.GetConfig`, multi-tenant filters, or cache adapters.

## Key Files
| File | Description |
|------|-------------|
| `WorkerSqlSugarRepository.cs` | `WorkerSqlSugarRepository<TEntity> : SimpleClient<TEntity>, ISqlSugarRepository<TEntity>`. Constructor takes the injected `ISqlSugarClient` and casts it to `SqlSugarScope` for `SimpleClient.Context`. |

## For AI Agents

### Working in this directory
- Keep this class minimal — it must remain free of multi-tenant, soft-delete, audit, or cache logic. Any of those would either silently fail (no `App.User`) or leak across batch processing scopes.
- The framework's `SqlSugarRepository<>` is intentionally NOT registered in the worker; if calc code needs additional repository methods, add them here as plain SqlSugar queries.
- Registered as `Scoped` in `Extensions/SqlSugarExtensions.cs` against the open generic — do not re-register at startup.

## Dependencies
### Internal
- Implements `Poxiao.DatabaseAccessor.ISqlSugarRepository<>` from `framework/Poxiao.Extras.DatabaseAccessor.SqlSugar`.

### External
- `SqlSugar` (`SimpleClient`, `SqlSugarScope`, `ISqlSugarClient`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
