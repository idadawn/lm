<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# model

## Purpose
TypeScript interfaces for intermediate-data calculation log + color models. Live alongside the API functions (in `../intermediateData*.ts`) so types are colocated with the responses they describe.

## Key Files
| File | Description |
|------|-------------|
| `intermediateDataCalcLogModel.ts` | `IntermediateDataCalcLogQuery / Item / Page`, `IntermediateDataCalcStepItem`, `IntermediateDataJudgeStepItem`, `IntermediateDataExecutionTrace`, `IntermediateDataTraceValueItem` — full execution-trace tree for the calculation diagnostics drawer. |
| `intermediateDataColorModel.ts` | Color rule entity + query types for the conditional-formatting UI. |

## For AI Agents

### Working in this directory
- Field names map directly to backend SqlSugar entity columns (most `F_*` columns transformed to camelCase). Preserve optional `?` markers — backend often returns null for empty steps.
- `IntermediateDataExecutionTrace` is consumed by both list/detail views and the drawer that shows step-by-step calc/judge details — extend with care.

### Common patterns
- Pure interfaces; pagination uses `{ list, pagination: { currentPage, pageSize, total } }` — matches `componentSetting.table.fetchSetting`.

## Dependencies
### Internal
- Consumed by `../intermediateData.ts` and related modules.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
