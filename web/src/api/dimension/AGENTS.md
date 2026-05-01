<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dimension

## Purpose
公共维度 (metric-dimension) CRUD + lookup. Drives the dimension management page and the dimension dropdowns embedded in metric/metric-composite forms.

## Key Files
| File | Description |
|------|-------------|
| `model.ts` | `getDimensionList` (POST list), `deleteDimension`, `getDimensionOptionsList`, `addDimension`, `updateDimension`, `getMetricSchema`, `getDimensionDetail`. Base path `/api/kpi/v1/metric-dimension`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `typing/` | TS interfaces for dimension query/result shapes (see `typing/AGENTS.md`). |

## For AI Agents

### Working in this directory
- The list endpoint is POST (not GET) — it accepts complex filter bodies.
- Several routes append id directly after a trailing slash (`/{id}`) — match the URL constants (`deleteDimension`, `updateDimension`).
- `getMetricSchema` returns the data-source options used to seed dimension creation.

### Common patterns
- `defHttp.post` for list/add, `defHttp.put` (with id-suffix) for update, `defHttp.delete` for remove.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
