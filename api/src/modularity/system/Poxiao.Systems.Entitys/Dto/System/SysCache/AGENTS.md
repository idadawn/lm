<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SysCache

## Purpose
DTOs for the 系统缓存管理 admin tool — exposes Redis cache entries to the system management UI for inspection and ops. Used by the SysCache controller to render a cache list and a single-key detail view.

## Key Files
| File | Description |
|------|-------------|
| `CacheListOutput.cs` | Cache list row: `name`、`overdueTime`、`cacheSize`. |
| `CacheInfoOutput.cs` | Single-entry detail view: `name`、`value` (raw cache content). |

## For AI Agents

### Working in this directory
- Pure read-side DTOs — no input DTOs because the controller takes simple `name` strings via query params.
- `value` is a string; large cache payloads should be truncated at the service layer before populating this DTO.
- Always tag with `[SuppressSniffer]`; namespace `Poxiao.Systems.Entitys.Dto.SysCache`.

### Common patterns
- camelCase properties matching frontend.
- `cacheSize` is `long` (bytes); `overdueTime` is non-nullable `DateTime`.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Consumed by the cache management endpoints (Redis cache provider behind `Poxiao.Cache`).
### External
- None directly.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
