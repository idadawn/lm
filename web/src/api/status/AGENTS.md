<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# status

## Purpose
价值链状态 (metric-covstatus) CRUD + dropdown options. Drives the "状态" management page and the status-select widgets shown on metric value-chain forms.

## Key Files
| File | Description |
|------|-------------|
| `model.ts` | `getStatusList`, `deleteStatus`, `getStatusOptionsList`, `addStatus`, `updateStatus`, `getStatusDetail`. Base `/api/kpi/v1/metric-covstatus`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `typing/` | Type interfaces for status payloads (see `typing/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Schema mirrors `dimension/model.ts` (sibling concept) — share helpers if extracted later.
- List endpoint is POST (filter body), options endpoint is GET (lightweight dropdown).

### Common patterns
- Same CRUD + options shape as `dimension/`.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
