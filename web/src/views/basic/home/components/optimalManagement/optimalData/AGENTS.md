<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# optimalData

## Purpose
ECharts `OptionsData` preset bundles consumed by `optimalManagement/index2.vue` and `homeComponents/index.vue`. Each file exports a `basicProps`/`histogramProps`/`mapProps`/`basicProps[2-4]` Vue prop descriptor whose `OptionsData.default` is a ready-to-render echarts option (line / horizontal bar / pie / gauge / histogram / map).

## Key Files
| File | Description |
|------|-------------|
| `props.ts` | `basicProps` — stacked-line option (category xAxis, dual yAxis) |
| `props2.ts` | Horizontal bar variant |
| `props3.ts` | Pie variant |
| `props4.ts` | Gauge variant |
| `histogramProps.ts` | Histogram / 柱状图 option preset |
| `mapProps.ts` | China map option preset |

## For AI Agents

### Working in this directory
- These objects are shared by reference — consumers MUST `JSON.parse(JSON.stringify(...))` before mutating titles/series to avoid cross-chart contamination.
- The `BasicProps` interface declares only `width`/`text`/`height` but the actual exported descriptor includes `OptionsData`. The interface is decorative; do not rely on it for type checking the runtime shape.
- `function calc()` at the bottom of `props.ts` is dead code (`throw new Error('Function not implemented.')`). Safe to remove if unreferenced repo-wide.

### Common patterns
- `default: { title:{text:'****率'}, tooltip, grid, legend, xAxis, yAxis, series:[] }` — backend payload fills `text/legend.data/xAxis.data/series` at runtime.

## Dependencies
### External
- `vue` (`PropType`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
