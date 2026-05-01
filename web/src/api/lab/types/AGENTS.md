<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
Concrete request/response interface contracts for the heaviest lab modules (rawData import wizard, Excel templates, intermediate-data formula, metric definitions). Centralized so the multi-step wizard and several views share a single type source.

## Key Files
| File | Description |
|------|-------------|
| `rawData.ts` | `ImportSession`, `ImportStrategy` (`'append'` / `'overwrite'`), `RawDataRow`, `Step1UploadAndParseInput/Output`, `Step2ProductSpecInput`, `Step3AppearanceFeatureInput`, `Step4ReviewOutput`, `Step4ReviewDataPage`, `DataPreviewResult`, `ImportLog`, `SimpleImportInput/Output`. |
| `excelTemplate.ts` | Excel template metadata + import/export shapes. |
| `intermediateDataFormula.ts` | Formula CRUD payloads + execution metadata. |
| `metric.ts` | Metric definition entity + filter/dropdown shapes. |

## For AI Agents

### Working in this directory
- Keep `ImportStrategy` literal union (`'append' | 'overwrite'`) authoritative — UI radios + backend depend on these literals.
- `prodDate` vs `prodDateStr` — backend returns both; the `Str` form is the formatted version for display. Preserve the dual-field pattern when adding date fields.
- Step1-4 input/output shapes mirror the backend `RawDataImportSessionService` step methods; align field-by-field on changes.

### Common patterns
- All optional response fields use `?` — backend may omit them when not applicable to the strategy.

## Dependencies
### Internal
- Consumed by `../rawData.ts`, `../excelTemplate.ts`, `../intermediateDataFormula.ts`, `../metric.ts`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
