<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
SheetJS-backed Excel I/O implementation.

## Key Files
| File | Description |
|------|-------------|
| `Export2Excel.ts` | Pure helpers: `jsonToSheetXlsx`, `aoaToSheetXlsx`, `jsonToMultipleSheetXlsx`, `aoaToMultipleSheetXlsx`. `setColumnWidth` infers `!cols` widths from cell content. |
| `ImportExcel.vue` | Hidden `<input type="file" accept=".xlsx,.xls">` triggered by `handleUpload`. Reads via `FileReader.readAsArrayBuffer` → `XLSX.read({ type:'array', cellDates:true })`. `getHeaderRow` decodes range and walks the first row; `shapeWorkSheel` fills empty cells with placeholders. Emits `success`/`error`/`cancel`. |
| `ExportExcelModal.vue` | Modal that prompts for filename + sheet name before invoking the JSON-to-sheet exporter. |
| `typing.ts` | `JsonToSheet`, `AoAToSheet`, `JsonToMultipleSheet`, `AoaToMultipleSheet`, `ExcelData`. |

## For AI Agents

### Working in this directory
- `ImportExcel` resets `inputRef.value = ''` on every dialog to allow re-selecting the same file — keep this fix.
- Cancel detection uses a `focus` listener with a 1s `setInterval`; do not remove the `cancelRef` flag flow.
- All filenames default to `excel-list.xlsx`; sheet names default to `sheet`/`sheet${i}`.

### Common patterns
- `header` parameter is unshifted into the data array and `skipHeader` is forced for json-to-sheet to avoid double headers.

## Dependencies
### Internal
- `/@/utils/dateUtil` (dateUtil for optional `dateFormat` formatting).
### External
- `xlsx` (utils, writeFile, read), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
