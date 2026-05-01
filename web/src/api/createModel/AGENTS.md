<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# createModel

## Purpose
KPI 建模 (metric / dashboard / chart-data) API surface. Targets `/api/kpi/v1/metricgot`, `/api/kpi/v1/metricdash`, and `/api/kpi/v1/metric-dimension` for tree CRUD, dashboard layout save/load, dimension/option lookups, and chart-data fetch.

## Key Files
| File | Description |
|------|-------------|
| `model.ts` | `getDashTreeList`, `createDash`, `editDash`, `deleteDash`, `getLayout`, `saveLayout`, `getChartData`, `getFilterData`, `getDimensions`, `getMetricsDimensions`, `getMarkAreaData`. Mixes real `dev` endpoints with `fastmock` placeholders. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `typing/` | Type interfaces for model parameters (see `typing/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `metricTag` is joined with `,` before posting (`createDash`) — backend expects a CSV string. Preserve.
- Some endpoints still hit `fastmock.site` — replace with `dev` server URLs once backend implementation lands.
- Naming straddles `dash/dashboard` and `metric/metricgot` — match backend route table when adding entries.

### Common patterns
- POST returns ids; PUT/DELETE address by appended `/{id}`.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
