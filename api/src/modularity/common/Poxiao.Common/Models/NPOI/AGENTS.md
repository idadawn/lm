<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# NPOI

## Purpose
Excel export configuration models for the NPOI-based exporter (`Security/ExcelExportHelper.cs`). Defines per-column styling, sheet-level theming and template / parameter shapes used to drive Excel file generation.

## Key Files
| File | Description |
|------|-------------|
| `ExcelConfig.cs` | Sheet-level config — `FileName`、`Title`、title/head/cell font/size/height、fore/back color、`IsAllSizeColumn`、`ColumnModel[]`. |
| `ExcelColumnModel.cs` | Column-level config — `Column`、`ExcelColumn` (header)、`Width`、`ForeColor`、`Background`、`Font`、`Point` (font size)、`Alignment` (`left`/`center`/`right`/`fill`/`justify`/`centerselection`/`distributed`). |
| `ExcelTemplateModel.cs` | Server-side template-driven export model. |
| `ParamsModel.cs` | Generic params bag for template substitution. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Models.NPOI`.
- Color fields are `System.Drawing.Color` — `using System.Drawing;` required at top of consumer files.
- `Alignment` is a string-coded enum (legacy) — see the XML doc on `ExcelColumnModel.Alignment` for the closed list of valid values; do not change to a typed enum without migrating the exporter.
- PascalCase property names here (export-side legacy convention) — stay consistent.

### Common patterns
- Composition: `ExcelConfig` aggregates `List<ExcelColumnModel>`.
- Legacy version banner comments (`版 本：V3.0.0` / `2017.03.09`).

## Dependencies
### Internal
- Consumed by `Security/ExcelExportHelper.cs` and `Security/ExcelImportHelper.cs`.
### External
- `System.Drawing` (`Color`).
- NPOI / Aspose.Cells (via the helpers).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
