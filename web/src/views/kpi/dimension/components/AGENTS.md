<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`dimension` 页面专用展示子组件，与 `kpi/createModel/components/` 内容完全相同（镜像副本）。提供归因分析与图表渲染容器，用于在维度详情页展示该维度下的指标趋势/对比。

## Key Files
| File | Description |
|------|-------------|
| `chartsModel.vue` | 接收 `chartsFlag/chartsData/gtData/ltData/random`，按 flag 渲染 BasicTable 或多组 ChartsManageMent 图表。 |
| `AttributionAnalysis.vue` | 时间范围 + 分析维度多选 + `getIndexChartsDataList` 触发，渲染 chartsModel。 |

## For AI Agents

### Working in this directory
- **此目录与 `views/kpi/createModel/components/` 是镜像副本**——修改一处务必同步另一处，或重构为共享组件抽到 `/@/components/`。
- 与 `views/basic/home/components/optimalManagement` 强耦合，重构 home 时要联动。

### Common patterns
- `chartsFlag === 'gudge'` 走 BasicTable，其它走 ChartsManageMent
- 通过 `random` ref 强制图表组件 rerender

## Dependencies
### Internal
- `/@/views/basic/home/components/optimalManagement`, `/@/components/Table`, `/@/api/basic/charts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
