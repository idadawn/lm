<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
Shared composables that runtime widgets in `../../Portal/*` use to fetch data, build ECharts options, and assemble Ant table props. Centralizes the option/dataType watch logic so each widget stays a thin shell.

## Key Files
| File | Description |
|------|-------------|
| `useCommon.ts` | `useCommon(activeData)` — generic data resolver for non-chart widgets (carousel, rankList, timeAxis, image, iframe, dataBoard, commonFunc). Handles static vs dynamic (`dataType`), `getDataInterfaceRes` calls, list shaping, returns reactive state plus the shared `CardHeader` and `webLink` components. |
| `useEChart.ts` | `useEChart(activeData, chartRef)` — wires `useECharts` to the widget, watches `activeData.option`/`dataType`, builds option per `jnpfKey` (bar/line/pie/...) using `chartData` defaults from `dataMap`. Provides drill-down `emitter` injection for map charts. |
| `useTable.ts` | `useTable(activeData)` — assembles `getTableBindValues` (columns, height, scroll, pagination, border) for `notice`/`tableList`/`rankList` widgets, fetches via `getDataInterfaceRes` or `getNoticeList`, formats dates with `dayjs`. |

## For AI Agents

### Working in this directory
- These hooks return the `CardHeader` symbol so widgets can `const { CardHeader, ... } = useXxx(...)` and use it directly in templates — keep that idiom.
- `arrayList` (`['carousel','rankList','timeAxis']`) inside `useCommon` gates list-shaped responses; update when extending list-style widgets.
- `useTable` adds an automatic 序号 column (`filedName: 'index'`) when `tableIndex` is set — preserve the index injection order.

### Common patterns
- `reactive` + `toRefs` to expose state ergonomically.
- `watch(activeData.option, ..., { deep: true })` is standard.

## Dependencies
### Internal
- `../../Portal/CardHeader/index.vue`, `../../Portal/Link/index.vue`
- `/@/api/systemData/dataInterface`, `/@/api/onlineDev/portal`
- `/@/hooks/setting`, `/@/hooks/web/useECharts`

### External
- `vue`, `echarts`, `dayjs`, `@vueuse/core` (`useElementBounding`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
