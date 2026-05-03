<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Repositories

## Purpose
Tenant-aware SqlSugar repository contract and implementation. Extends SqlSugar's `SimpleClient<TEntity>` and resolves the active connection per request — for `SCHEMA` isolation it switches the `SqlSugarScope` connection by `TenantId`; for `COLUMN` isolation it captures the `IsolationField` so query filters can apply.

## Key Files
| File | Description |
|------|-------------|
| `ISqlSugarRepository.cs` | Generic `ISqlSugarRepository<TEntity> : ISimpleClient<TEntity>` marker for DI. |
| `SqlSugarRepository.cs` | Implementation: reads `ConnectionStringsOptions` + `TenantOptions`, fetches `GlobalTenantCacheModel` from `ICacheManager` (`poxiao:globaltenant`), and switches `Context` via `AsTenant().GetConnectionScope`. Throws `Oops.Oh("数据库连接错误")` on invalid connections. |

## For AI Agents

### Working in this directory
- Tenant resolution depends on the JWT `TenantId` claim plus the cache key `poxiao:globaltenant`; never read the claim directly outside this layer.
- `[AllowAnonymous]` endpoints intentionally skip tenant switching — keep that branch.
- Throw via `Oops.Oh` (FriendlyException) for any tenant misconfiguration so the API surface stays consistent.

### Common patterns
- Constructor uses a transient service scope to resolve `ICacheManager` — do not promote those services to the constructor's signature.
- `base.Context = (SqlSugarScope)context` is required because `SimpleClient` types `Context` as `ISqlSugarClient`.

## Dependencies
### Internal
- `Models.GlobalTenantCacheModel`, `Options.ConnectionStringsOptions`/`TenantOptions`, `Extensions.PoxiaoTenantExtensions`, `Poxiao.Infrastructure.Manager.ICacheManager`, `Poxiao.FriendlyException.Oops`.
### External
- `SqlSugarCore.SimpleClient<>`, `Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
