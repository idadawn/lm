<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Internal DTOs used by the SqlSugar accessor's paging helpers. Defines the wire shape returned by `ToPagedList` extensions.

## Key Files
| File | Description |
|------|-------------|
| `SqlSugarPagedList.cs` | `Pagination { CurrentPage, PageSize, Total }` and `SqlSugarPagedList<TEntity> { pagination, list }`. |

## For AI Agents

### Working in this directory
- Property casing here is intentionally lowercase (`pagination`, `list`) to match the established frontend JSON contract — do **not** rename without coordinating with `web/src/api`.
- Keep these DTOs free of methods/attributes; they are returned directly to controllers.

### Common patterns
- `Total` is `int` — for very large tables consider whether a `long` overload is needed before changing.

## Dependencies
### External
- BCL only.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
