<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ExportImport

## Purpose
Large helper for serialising and re-importing visualdev model definitions and form data as JSON packages. Handles model metadata, permissions, dictionary references, and bound API/data interfaces so an exported package can be re-imported on another tenant.

## Key Files
| File | Description |
|------|-------------|
| `ExportImportDataHelper.cs` | Single class containing the export/import pipeline. ~46k tokens — too large for inline review; rely on method-name navigation. |

## For AI Agents

### Working in this directory
- The file is intentionally monolithic so consumers only need to wire one helper. If you must split it, use `partial class ExportImportDataHelper` rather than renaming or splitting types.
- When adding new VisualDev types to the export schema, mirror the read path so import remains symmetric.

### Common patterns
- Reads/writes JSON via Newtonsoft.Json with the `CommonConst.options` settings.

## Dependencies
### Internal
- `Poxiao.VisualDev` runtime types, `Poxiao.Common` helpers (`JsonHelper`, `SnowflakeIdHelper`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
