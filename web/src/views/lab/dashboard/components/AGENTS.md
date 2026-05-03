<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
驾驶舱图表组件库：KPI 卡片 + 5 类核心图表 + AI 助手。每个组件自包装数据请求与 ECharts 实例。

## Key Files
| File | Description |
|------|-------------|
| `KpiCards.vue` | 顶部关键指标卡片（产量/合格率/不良率等）|
| `QualityDistribution.vue` | 合格/不合格分布饼图 |
| `LaminationTrend.vue` | 层压厚度/质量趋势折线 |
| `DefectTop5.vue` | Top5 缺陷类别柱状 |
| `ProductionHeatmap.vue` | 班次/日期生产热力图 |
| `ThicknessCorrelation.vue` | 厚度相关性散点 |
| `AiAssistant.vue` | AI 问答助手（已被父页移除挂载，但保留组件）|

## For AI Agents

### Working in this directory
- 接收 `start-date` / `end-date` props，watch 触发数据刷新。
- 暴露 `refresh`/`reload` 给父页 ref 调用；ECharts 实例在 `onUnmounted` 时 dispose。
- 图表大小响应窗口 resize：使用 ResizeObserver 或全局 resize 事件。

### Common patterns
- `<script setup lang="ts">` + `defineExpose({ refresh })`.
- API 调用统一带 loading 状态，外部错误时展示 a-empty。

## Dependencies
### Internal
- `/@/api/lab/dashboard`
### External
- `echarts`, `ant-design-vue`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
