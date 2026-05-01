<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# chart

## Purpose
Visual-dev / chart-builder API client. Targets the `visualdev` mock endpoints (nodes, elements, optimal/warning nodes, indicator data) plus the KPI metric-mind-map (`/api/kpi/v1/metricgot`) endpoints used by the indicator tree and value-chain views.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `getNodes`, `getNodeElements`, `getOptimalNodeElements`, `getWarningNodeElements`, `addIndicator`, `putIndicator`, `getIndicatorTreeList`, `addIndicatorValueChain`, etc. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `typing/` | Ambient `API.*` interfaces for chart request/result shapes (see `typing/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Two-target module — visual-dev mock endpoints (`mock`) and real KPI endpoints (`dev=''`). Keep `mock` vs `dev` URL prefixes intact when extending.
- `addIndicator` / `putIndicator` use the same `/api/kpi/v1/metricgot` base — backend disambiguates via verb + id segment.

### Common patterns
- Mixed `defHttp.post / get / put / delete` with backend-issued ids appended to URL.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
