<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Common.CodeGen

## Purpose
Runtime helpers for the visual code-generator / online forms feature ("可视化开发"). Provides the control-data parser that converts raw Dictionary rows into display-ready dictionaries (resolving popupSelect / relationForm / usersSelect controls against the SqlSugar repository) and a JSON template export/import helper. Lives in its own project because it depends on `Poxiao.VisualDev` runtime types that the lighter-weight `Poxiao.Common` cannot reference.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Common.CodeGen.csproj` | Project reference to `Poxiao.VisualDev`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `DataParsing/` | `ControlParsing` — translates form-control values via cache + repository (see `DataParsing/AGENTS.md`) |
| `ExportImport/` | `ExportImportDataHelper` — large helper for serialising/deserialising visualdev models (see `ExportImport/AGENTS.md`) |

## For AI Agents

### Working in this directory
- This project depends on `Poxiao.VisualDev`. Don't push references the other way (Visualdev should not depend on CodeGen).
- Cache keys reuse `CommonConst.CodeGenDynamic` (`codegendynamic_`) plus suffixes; respect that prefix.

### Common patterns
- Heavy use of `_cacheManager` (`ICacheManager`) with a 5–10 minute TTL.
- Resolution paths are switched on `PoxiaoKeyConst.*` constants (USERSSELECT / POPUPSELECT / RELATIONFORM / etc.).

## Dependencies
### Internal
- `Poxiao.VisualDev` (project), `Poxiao.Common` runtime types, `Poxiao.Systems.Entitys`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
