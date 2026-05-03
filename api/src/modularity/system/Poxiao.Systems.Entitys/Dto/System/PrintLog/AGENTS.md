<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PrintLog

## Purpose
DTOs for "打印日志" — paged read-only audit log of print operations (which template printed which record, by whom, when). Paired with the `PrintDev` print template feature.

## Key Files
| File | Description |
|------|-------------|
| `PrintLogQuery.cs` | Paged grid query, inherits `Poxiao.Infrastructure.Filter.PageInputBase`; adds `startTime` / `endTime` (Unix timestamp, `long?`) for date-range filtering. |
| `PrintLogOutuut.cs` | Per-row output projection. **Note: filename has a typo (`Outuut` instead of `Output`)** — keep the typo to avoid touching `[FromBody]` model binding paths. |

## For AI Agents

### Working in this directory
- The misspelled filename `PrintLogOutuut.cs` is intentional from the original codebase; renaming requires a coordinated change across services and CodeGen templates that may reference the type by file/path.
- Time filters use Unix `long?` (milliseconds) rather than `DateTime` to match the frontend grid's date picker output. Convert in the service layer.
- Namespace `Poxiao.Systems.Entitys.Dto.System.PrintLog` (note: includes `.System.` segment).
- No `Cr` / `Up` DTOs — the log is append-only and written by the print pipeline, not the user.

### Common patterns
- Inherit `PageInputBase` for any paged grid query.
- Use `long?` for date-range filters bound to the frontend's millisecond timestamps.

## Dependencies
### Internal
- Inherits `Poxiao.Infrastructure.Filter.PageInputBase`.
- Written/read by `modularity/system/Poxiao.Systems` print-log service triggered by `PrintDev` print actions.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
