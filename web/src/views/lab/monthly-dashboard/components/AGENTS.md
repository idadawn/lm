<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
月度驾驶舱图表组件库：KPI 卡、层压趋势、合格分布饼、班次对比雷达/柱、产量热力、厚度散点、不合格 Top5、明细表、AI 对话。

## Key Files
| File | Description |
|------|-------------|
| `KpiCards.vue` | 月度 KPI 卡片（汇总 + 每日产量）|
| `LaminationTrendChart.vue` | 层压趋势折线 |
| `QualityDistributionPie.vue` | 合格/不合格分布（按列分类）|
| `QualityTrendChart.vue` | 质量趋势 |
| `ShiftComparisonChart.vue` 之外另存于 `monthlyReport`； `ShiftComparisonRadar.vue` | 班次雷达 |
| `ProductionShiftHeatmap.vue` | 班次产量热力 |
| `ThicknessCorrelationScatter.vue` | 厚度相关散点 |
| `UnqualifiedTop5.vue` | 不合格 Top5 |
| `MonthlyDetailsTable.vue` | 明细表 |
| `ChatAssistant.vue` | AI 对话助手（驻留侧边）|

## For AI Agents

### Working in this directory
- 图表均为受控：父页注入 `data` + `loading` + `report-configs`，子组件只渲染。
- `ChatAssistant.vue` 维护对话上下文，调用后端 NLQ/对话接口；切勿在主图表组件中混入对话逻辑。

### Common patterns
- `defineExpose({ resize })` 父页窗口变化时统一调用。

## Dependencies
### Internal
- `/@/api/lab/monthly-dashboard`, `/@/api/ai`
### External
- `echarts`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
