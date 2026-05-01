<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HMapChart

## Purpose
ECharts geo-map widget with drill-down. Renders a chart container, plus a breadcrumb when the user has drilled into nested regions, and an optional bar-title overlay (`styleType == 4`).

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Holds `chartRef` and a `state.hashMap` of drill-down levels rendered as `<breadcrumb>`. Title overlay text styled via `barTitleTextStyle*`. Background color via `activeData.option.bgColor`. |

## For AI Agents

### Working in this directory
- The drill-down state lives in this component (not the hook) because the breadcrumb UI is map-specific. Coordinate with `useEChart` for `emitter`-based events.
- Map options like `styleType == 4` (bar overlay) are unique to this widget; do not copy logic into other chart widgets.
- Click handler `readyMap(key, value)` rewinds the breadcrumb stack — preserve order semantics.

### Common patterns
- `state.hashMap = new Map()` for ordered drill levels.
- Inline `:style` for breadcrumb fonts.

## Dependencies
### Internal
- `../../Design/hooks/useEChart`, `../CardHeader`

### External
- `ant-design-vue` (Breadcrumb), `echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
