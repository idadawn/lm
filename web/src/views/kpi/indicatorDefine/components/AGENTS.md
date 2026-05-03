<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`indicatorDefine` 页面专用子组件：归因分析、维度贡献排行（图+表）、目标新增弹窗、图表模型预览。仅服务于父页面，不在此外被引用。

## Key Files
| File | Description |
|------|-------------|
| `AttributionAnalysis.vue` | 指标归因分析视图 |
| `DimensionContributionRanking.vue` | 维度贡献排行容器（包含图表+列表两段） |
| `DimensionContributionRankingChart.vue` | 维度贡献排行图表（ECharts） |
| `DimensionContributionRankingList.vue` | 维度贡献排行明细表 |
| `chartsModel.vue` | 指标图表样例/模板预览 |
| `targetAdd.vue` | 目标值新增弹窗 |

## For AI Agents

### Working in this directory
- 这些组件耦合 `indicatorDefine` 上下文，避免在其他页面复用；如需复用先抽到 `/@/components`。
- 图表通过 ECharts 实例，注意 props 变化时 `dispose` + 重建以避免内存泄漏。

### Common patterns
- props 接收 `record`/`indicator`，emit `register`/`reload` 给父页。
- 列表组件复用项目 `BasicTable`；图表组件直接使用 `echarts`。

## Dependencies
### Internal
- `/@/components/Table`, `/@/components/Modal`
- 父级 `../Form*.vue`、`../datamodels.ts`
### External
- `echarts`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
