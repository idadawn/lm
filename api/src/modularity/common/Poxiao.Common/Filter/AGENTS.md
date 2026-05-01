<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Filter

## Purpose
Standard pagination/filter input and output contracts shared by every controller in the platform. Defines the canonical `PageInputBase` request DTO and `PageInfo`/`PagedResultDto`/`PageResult` response shapes that wrap SqlSugar's `SqlSugarPagedList<T>`.

## Key Files
| File | Description |
|------|-------------|
| `PageInputBase.cs` | Standard list input — `keyword` (via `KeywordInput`), `queryJson`, `currentPage` (default 1), `pageSize` (default 50), `sidx`, `sort` (default "desc"), `menuId`. |
| `PageInfo.cs` | Pagination response triple — `currentPage`、`pageSize`、`total`. Also defines `PagedResultDto<T>` (PascalCase) and `PageResult<T>` (camelCase) wrappers, plus a `SqlSugarPageResult` adapter from SqlSugar's pagination via Mapster. |
| `KeywordInput.cs` | Tiny base with a single nullable `Keyword` property. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Filter`.
- New list endpoints **must** inherit `PageInputBase` rather than redefining pagination fields; this keeps the frontend contract uniform.
- `PageResult<T>` (camelCase) is the legacy frontend-facing wrapper; `PagedResultDto<T>` (PascalCase) is the newer style. Don't introduce a third wrapper.
- `SqlSugarPageResult` uses Mapster (`Adapt<PageInfo>`) — keep the cast path stable.
- All classes carry `[SuppressSniffer]` and `[JsonProperty(...)]` annotations to lock JSON casing — preserve both.

### Common patterns
- camelCase JSON fields enforced via `[JsonProperty]`.
- Defaults set via property initializers (page=1, size=50, sort=desc).

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Mapster (`Adapt`) — used by `SqlSugarPageResult`.
### External
- Newtonsoft.Json (`[JsonProperty]`).
- SqlSugar (`SqlSugarPagedList`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
