<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# typing

## Purpose
Ambient TypeScript declarations for the chart/visual-dev API surface. Declared inside `declare namespace API { ... }` so call sites reference `API.GetNodesParams`, `API.GetChartsResult`, etc. without explicit imports.

## Key Files
| File | Description |
|------|-------------|
| `model.d.ts` | `BaseResult`, `GetNodesParams/Result`, `GetNodeElementsParams/Result`, `GetOptimalNodeElementsParams`, etc. — describes node/edge/element shapes used by the indicator-tree visualizations. |

## For AI Agents

### Working in this directory
- Ambient declarations — adjust `tsconfig` includes if the file path changes.
- Adding a new request/result type: keep names consistent with the function in `../index.ts` (e.g. `Get*Params` ↔ `Get*Result`).

### Common patterns
- All result shapes extend `BaseResult { version, code, msg, data }`.

## Dependencies
### Internal
- Consumed by `../index.ts`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
