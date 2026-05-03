<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# UnitOfWork

## Purpose
SqlSugar-backed implementation of the `IUnitOfWork` contract used by the `[UnitOfWork]` MVC filter. Wraps SqlSugar's `AsTenant()` transaction APIs so `[UnitOfWork]`-decorated controller actions get implicit BeginTran / CommitTran / RollbackTran semantics.

## Key Files
| File | Description |
|------|-------------|
| `SqlSugarUnitOfWork.cs` | `sealed class SqlSugarUnitOfWork : IUnitOfWork` — `BeginTransaction`/`CommitTransaction`/`RollbackTransaction`/`OnCompleted` delegate to `_sqlSugarClient.AsTenant()`. `OnCompleted` calls `Dispose()` on the scope. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.DatabaseAccessor` (note: not `Poxiao.Infrastructure.*`) — keeps it close to other DB-accessor abstractions.
- `_sqlSugarClient` is cast to `SqlSugarScope` in the ctor; if the registered `ISqlSugarClient` is ever swapped to a different concrete type the cast will fail — guard there before changing DI registration.
- Use `AsTenant()` (SqlSugar tenant API) for transactions to cover multi-database scenarios (SQL Server / MySQL / Oracle as configured); a plain `Ado.BeginTran()` would NOT span tenants.
- Do not add domain logic here — feature transactional logic belongs in module services that opt into `[UnitOfWork]`.

### Common patterns
- One `*UnitOfWork` impl per ORM; this is the only one currently wired.

## Dependencies
### Internal
- `Poxiao` framework — `IUnitOfWork`, `UnitOfWorkAttribute`.
### External
- `SqlSugar` (`ISqlSugarClient`, `SqlSugarScope`).
- `Microsoft.AspNetCore.Mvc.Filters` (`FilterContext`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
