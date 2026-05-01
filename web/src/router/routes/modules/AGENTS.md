<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# modules

## Purpose
Static, build-time route modules merged into `basicRoutes`. Currently hosts the laboratory cockpit / metric / monthly-report routes and the AI (NLQ chat) entry — these are shipped as code rather than coming from the backend menu, because they're either core fixed pages or pre-released features.

## Key Files
| File | Description |
|------|-------------|
| `lab.ts` | `/lab` parent route under `LAYOUT`. Children: `monthly-dashboard` (生产驾驶舱), `metric` / `metric/form/:id?`, `intermediate-data-formula` (公式维护), `intermediate-data-judgment-level` (判定等级), `monthly-report` (月度质量报表), `magnetic-data` (磁性数据), `report-config` (指标列表). Default redirect to `monthly-dashboard`. |
| `ai.ts` | `/ai` parent route — exposes the NLQ-agent chat view (`web/src/views/ai/...`). |

## For AI Agents

### Working in this directory
- Pages targeted by these routes live under `web/src/views/lab/*` / `web/src/views/ai/*`.
- `affix: true` on `monthly-dashboard` keeps it pinned in multi-tab; preserve when refactoring.
- Child paths that should not appear in menus must set `meta.hideMenu = true` (see `MetricForm`).

### Common patterns
- `path: 'something'` (no leading slash) — joined to parent via `joinParentPath` in `helper/menuHelper.ts`.
- `meta.icon` follows the `ant-design:*` / `ion:*` icon naming used elsewhere.

## Dependencies
### Internal
- `/@/router/constant` (LAYOUT), `/@/hooks/web/useI18n`, `/@/router/types`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
