<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# index

## Purpose
"生产驾驶舱" (production cockpit) — the home tab. Mobile adaptation of `web/src/views/lab/monthly-dashboard/index.vue`. Shows date-range KPI tiles, quality distribution, lamination-coefficient trend, top-5 nonconforming items, shift comparison, and a thickness-vs-lamination scatter plot. Supports pull-to-refresh.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Dashboard layout: skeleton overlay during load, `picker mode="date"` start/end range, refresh button, four KPI cards (检测总重量/合格率/叠片系数均值/今日产量 with day-over-day delta), quality distribution bars, then card sections drawn with native `<canvas>`. Aggregates data from `getMonthlyReport`, `getLaminationTrend`, `getThicknessCorrelation`, `getDailyProduction`. |

## For AI Agents

### Working in this directory
- Charts are deliberately rendered with native `<canvas>` (not echarts/uCharts) to keep the apk small — preserve this when adding visualizations.
- Date range defaults: start = `getStartOfMonth()`, end = `getToday()` from `@/utils/date.js`. Honor pull-down-refresh by calling `fetchData(true)`.
- KPI color thresholds use `getRateColor()`; keep thresholds in sync with the Web dashboard.
- Empty state (`totalWeight === 0`) must remain a graceful no-op, not an error toast.

### Common patterns
- `cardStyle(index)` applies staggered fade-in animation tied to `kpi-loaded` class.
- Numbers formatted via per-page `formatNumber()` (thousands separators).

## Dependencies
### Internal
- `@/api/dashboard.js` (all four endpoints).
- `@/utils/date.js` for date helpers.

### External
- uni-app `picker`, native `<canvas>` API.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
