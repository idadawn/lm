<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Helpers

## Purpose
Pure-logic helpers shared between the API process and the standalone calculation Worker. Covers Excel-style formula parsing, range/aggregate functions, raw→intermediate data shaping, furnace-number parsing, magnetic-data import dedup, and detection-data JSON conversion. **No DI for `IHttpContextAccessor`/`ICacheManager` here** — keep these decoupled from request scope.

## Key Files
| File | Description |
|------|-------------|
| `FormulaParser.cs` | `IFormulaParser` implementation (`ITransient`). Regex extractors for `[Var]`, `$Var`, `RANGE(prefix, start, end)`, `RANGE(prefix)`, `DIFF_FIRST/DIFF_LAST(prefix, count)`, and `[Start] TO [End]`. Used by `IntermediateDataFormulaBatchCalculator`. |
| `IntermediateDataGenerator.cs` | `ITransient`. Builds `IntermediateDataEntity` skeletons from raw rows (no calculation), maintains 1..N detection-column expansion. Uses `IntermediateDataFormulaCalcLogEntity` for trace logging. |
| `RangeFormulaCalculator.cs` | `ITransient`. Evaluates `AVG/SUM/MAX/MIN(RANGE(Thickness, 1, $DetectionColumns))` style formulas against entity instances via reflection. |
| `FurnaceNoHelper.cs` | Static thin wrapper around `Models/FurnaceNo` parsing — exposes `FurnaceNoParseResult` with line/shift/date/batch/coil/subcoil. |
| `MagneticImportHelper.cs` | Static. Groups `MagneticDataImportItem` by `(FurnaceNo, IsScratched)` and selects the preferred row when duplicates exist. |
| `DetectionDataConverter.cs` | Static JSON ↔ `Dictionary<int, decimal?>` for the detection-columns payload stored as JSON in raw/intermediate data. |

## For AI Agents

### Working in this directory
- Keep helpers stateless — no DI for caches/HTTP/user. The Worker re-uses these classes in a separate process.
- New formula syntax additions: extend `FormulaParser` regexes **and** `IFormulaParser.ExtractVariables`/`Calculate` in the interface contract.
- Furnace-number format: `[产线][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号][W?][特性后缀?]` (see `Models/FurnaceNo.cs`); parse via `FurnaceNo.Parse` first, then call `FurnaceNoHelper` if you need flat result objects.
- Detection columns are 1-indexed; `IntermediateDataGenerator.ParseDetectionColumns(n)` returns `[1..n]`.

### Common patterns
- `[Variable]` brackets and `$DynamicVar` (e.g. `$DetectionColumns`) compile via separate regexes.
- Ranges are inclusive; `Detection1..22`/`Thickness1..22`/`LaminationDist1..22` are reserved prefixes recognised across the calc helpers.
- All numeric work uses `decimal` (not `double`) and `MidpointRounding.AwayFromZero`.

## Dependencies
### Internal
- `Poxiao.Lab.Interfaces.IFormulaParser`.
- `Poxiao.Lab.Entity` (entities, attributes, enums, `Models/FurnaceNo`, MagneticData DTOs).
- `Poxiao.Lab.Entity.Config.LabOptions`.
### External
- `Newtonsoft.Json`, `System.Text.RegularExpressions`, `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
