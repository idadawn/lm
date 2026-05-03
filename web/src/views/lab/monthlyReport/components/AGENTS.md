<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
月度报表子组件：汇总卡、筛选面板、明细表、班组面板、质量趋势/班次对比/不合格分类图表、报告配置弹窗。

## Key Files
| File | Description |
|------|-------------|
| `SummaryCards.vue` | 顶部汇总指标卡片 |
| `FilterPanel.vue` | 日期/班次/班别/产品规格筛选面板（v-model）|
| `DetailTable.vue` | 明细表（左侧主区域）|
| `ShiftGroupPanel.vue` | 右侧班组统计面板 |
| `QualityTrendChart.vue` | 质量趋势 |
| `ShiftComparisonChart.vue` | 班次对比柱图 |
| `UnqualifiedCategoryChart.vue` | 不合格类别分布 |
| `ReportConfigModal.vue` | 报告字段配置弹窗（关联 reportConfig 模块）|

## For AI Agents

### Working in this directory
- 受控渲染：父页提供 `data`/`loading`/`*-columns`/`report-configs` 全量注入。
- `FilterPanel` 使用 `v-model:xxx` 多向绑定；不要把筛选逻辑迁到子组件内部。

### Common patterns
- `defineProps<{ data: T[]; loading: boolean }>()` 强类型。

## Dependencies
### Internal
- `/@/api/lab/monthlyReport`, `/@/api/lab/reportConfig`
### External
- `echarts`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
