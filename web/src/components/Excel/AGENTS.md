<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Excel

## Purpose
Excel import/export utilities backed by SheetJS (`xlsx`). Provides `ImportExcel.vue` (file picker → JSON), `ExportExcelModal.vue` (download config dialog), and pure functions `jsonToSheetXlsx`/`aoaToSheetXlsx` plus their multi-sheet variants. Used by laboratory data import flows and any list export action.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel: `ImportExcel`, `ExportExcelModal`, `jsonToSheetXlsx`, `aoaToSheetXlsx`, `jsonToMultipleSheetXlsx`, `aoaToMultipleSheetXlsx`, plus typings. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Implementation + types (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- All exports default `bookType: 'xlsx'`; pass `write2excelOpts` to switch formats.
- Auto column-width derives from the longest cell string, min 3 — pre-stringify dates/numbers before exporting to control width.
- `ImportExcel.vue` returns parsed `ExcelData[]` (header + results + sheetName) by default; pass `isReturnFile` to receive the raw `File` instead.

### Common patterns
- Date cells get a +43s offset when `timeZone === 8` (works around SheetJS issue #1470); preserve the magic offset.

## Dependencies
### Internal
- `/@/utils/dateUtil`.
### External
- `xlsx` (SheetJS), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
