<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
SqlSugar-side DI plumbing and query/expression helpers. Registers a singleton `ISqlSugarClient` (as `SqlSugarScope`), exposes paging extensions on `ISugarQueryable<T>`, declares JSON-aware SQL functions, and provides the `PoxiaoTenantExtensions` helpers that build per-tenant `ConnectionConfigOptions` (普通链接 / 自定义链接 / 主从).

## Key Files
| File | Description |
|------|-------------|
| `SqlSugarServiceCollectionExtensions.cs` | `AddSqlSugar` overloads — registers `ISqlSugarClient` (singleton) and `ISqlSugarRepository<>` (scoped). |
| `PagedQueryableExtensions.cs` | `ToPagedList(pageIndex, pageSize)` returning `SqlSugarPagedList<T>` with a populated `Pagination`. |
| `JsonSqlExtFunc.cs` | Declares an `ExpMethods` `SqlFuncExternal` table so `ToObject<T>(string)` can be used inside SqlSugar expressions. |
| `TenantLinkExtensions.cs` | `PoxiaoTenantExtensions.GetLinkToOrdinary` / `GetLinkToCustom` — convert `TenantLinkModel` rows into SqlSugar `ConnectionConfig` entries. |

## For AI Agents

### Working in this directory
- Treat the `ISqlSugarClient` registration as a singleton (`SqlSugarScope` is thread-safe per its own docs); do not switch to scoped.
- SQL-function placeholders such as `ToObject<T>` must throw `NotSupportedException` outside expressions — keep that contract.
- When adding tenant-link logic, reuse `App.GetOptions<ConnectionStringsOptions>` to find the `default` row and string-format `DefaultConnection` with the tenant DB name (existing convention).

### Common patterns
- Extensions live in the `SqlSugar` namespace (not `Microsoft.Extensions.DependencyInjection`) — keep that grouping.
- Pagination uses `int` `Total` (cast from `ref int totalCount`) for compatibility with `Pagination` DTO.

## Dependencies
### Internal
- `ConnectionConfigOptions`, `ConnectionStringsOptions`, `TenantLinkModel` from `../Options` and `../Models`.
### External
- `SqlSugarCore`, `Poxiao.App` for config resolution.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
