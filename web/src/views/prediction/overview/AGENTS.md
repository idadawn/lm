<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# overview

## Purpose
预测总览仪表盘：响应式三栏布局，组合多个 ECharts 图表展示站点分析、访问雷达、仪表盘、品类占比、访问来源等指标。500ms 后取消 loading 占位，所有数据目前由各组件内部模拟。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 容器页：组合 `SiteAnalysis` + `VisitRadar` + `VisitGauge` + `SalesProductPie` 等图，统一 `loading` |
| `props.ts` | 共享 `BasicProps`：`width`/`height`（默认 `100%`/`280px`） |
| `SiteAnalysis.vue` | 顶部站点分析卡片 |
| `VisitRadar.vue` | 访问雷达图 |
| `VisitGauge.vue` | 仪表盘 |
| `SalesProductPie.vue` | 种类占比饼图（`Card` + `useECharts`） |
| `VisitAnalysis.vue` / `VisitAnalysisBar.vue` / `VisitAxis.vue` / `VisitSource.vue` | 备用图表组件 |

## For AI Agents

### Working in this directory
- 所有图表通过 `/@/hooks/web/useECharts` + `chartRef` 渲染。
- 共享 `BasicProps` 控制尺寸；统一使用 `<Card>` 包装。
- 该目录与 `predictionManagement/prediction/` 几乎重复，修改时考虑同步或抽取共享组件。

### Common patterns
- `defineProps` 重复声明 `loading/width/height`（未使用 `props.ts` 的 spread）。

## Dependencies
### Internal
- `/@/hooks/web/useECharts`
### External
- `ant-design-vue`（Card）、`echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
