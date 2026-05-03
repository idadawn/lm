<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# homeComponents

## Purpose
Container that fetches the home charts list (`getHomeChartsDataList`) and renders one `chartsManageMent` (from `optimalManagement/index2.vue`) per element, switching the underlying option preset by chart type (axis / hengaxis / pie / gauge / map / histogram). Drives the dynamic part of `SiteAnalysis` tab1.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Async-fetches chart list, clones one of `basicProps[/2/3/4]` per element type, sets title/legend/xAxis/series, pushes into `state.basicParamsList`; flag 50 vs 100 controls `md:w-1/2` vs full width |

## For AI Agents

### Working in this directory
- The width flag (50/100) is decided by `v.type === 'pie' || v.type === 'gudge'` — note the typo `gudge` (intentional, matches API payload). Do not "fix" without coordinating with the backend.
- The cloning pattern (`JSON.parse(JSON.stringify(basicProps))`) is required because `basicProps` is shared at module scope; mutating in place corrupts other charts.
- Click bubbles to `jumpSecond` from the parent — keep wrapper `<div @click>` even on layout refactors.

### Common patterns
- Switch on `v.type` ('axis' | 'hengaxis' | 'pie' | 'gauge' | 'map' | 'histogram') to pick a `basicProps*` template.

## Dependencies
### Internal
- `../optimalManagement/index2.vue`, `../optimalManagement/optimalData/{props,props2,props3,props4}.ts`, `/@/api/basic/charts` (`getHomeChartsDataList`)
### External
- `vue`, `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
