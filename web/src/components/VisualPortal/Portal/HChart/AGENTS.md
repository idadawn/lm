<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HChart

## Purpose
Generic ECharts widget. Handles `barChart`, `lineChart`, `pieChart`, `funnelChart`, etc. by delegating to `useEChart`, which switches the option object based on `activeData.jnpfKey` and `activeData.option`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Thin shell — provides a `chartRef` div, calls `useEChart(activeData, chartRef)`, invokes `init()` `onMounted`. Uses the shared `<a-card class="portal-card-box">` chrome with `CardHeader`. |

## For AI Agents

### Working in this directory
- All chart-type logic lives in `../../Design/hooks/useEChart.ts`. Add new chart kinds there, then add a property panel in `../../Design/rightComponents/`.
- The chart wrapper has `class="h-full w-full box-inherit p-10px"` — chart resize relies on this padding plus `useECharts`' resize observer.
- This widget receives `CardHeader` from the hook destructure — do not re-import.

### Common patterns
- `chartRef` typed `Ref<HTMLDivElement | null>`.
- `defineProps(['activeData'])`.

## Dependencies
### Internal
- `../../Design/hooks/useEChart`

### External
- `vue`, `echarts` (transitively via `useECharts`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
