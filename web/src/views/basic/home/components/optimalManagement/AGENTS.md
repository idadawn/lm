<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# optimalManagement

## Purpose
寻优管理 (parameter optimisation) tab on the dashboard. `index.vue` fetches `getChartsDataList` and renders a list of chart cards using the shared `index2.vue` chart wrapper, which actually mounts the ECharts instance with `basicProps` from `optimalData/`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Lists `state.basicParamsList`, populates via `getChartsDataList({nodeId,userId})`, builds props from `optimalData/props.ts` per element |
| `index2.vue` | The actual chart card component receiving `basicProps` as props (used by both `index.vue` and `homeComponents/index.vue`) |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `optimalData/` | ECharts `OptionsData` presets per chart type (see `optimalData/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `index2.vue` is the single source of truth for rendering — both `homeComponents/` and this `index.vue` pass the same `basicProps` shape into it. Keep prop signatures stable.
- `state.tabs` array (`建模/预警/基线`) is declared but tab switching is currently driven outside this component.
- Always deep-clone `basicProps` before mutating per-element data — see same caveat in `homeComponents/AGENTS.md`.

### Common patterns
- `onMounted` → `getChartsDataListData()` → map response into `basicParamsList`.

## Dependencies
### Internal
- `./optimalData/props`, `/@/api/basic/charts` (`getChartsDataList`)
### External
- `vue`, `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
