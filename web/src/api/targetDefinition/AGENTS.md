<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# targetDefinition

## Purpose
指标定义 (metric / metric-kinship) CRUD plus list / detail / version-graph queries. Supports the indicator-management page where users configure measurable metrics and their hierarchical relationships.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `getMetrickinship`, `putMetrickinship`, `deleteMetrickinship`, `postMetrickinshipList` (`/metric/list`), plus tag/dimension/version/composite related fetches. Base `/api/kpi/v1/`. |

## For AI Agents

### Working in this directory
- "metrickinship" is backend's term for indicator definition (kinship == 谱系). Don't rename without backend coordination.
- `postMetrickinshipList` posts to `/metric/list` (different sub-path) — preserve the asymmetry.
- Some `put` calls have commented-out partial-payload code; keep current full-payload behaviour for backward compat.

### Common patterns
- Module is the consolidated CRUD entrypoint for indicator views — long file by design.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
