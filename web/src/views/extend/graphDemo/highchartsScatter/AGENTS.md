<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsScatter

## Purpose
Highcharts 散点图（Scatter）演示页面，按性别划分的身高 / 体重分布，是 `extend/graphDemo` 中体积较大的示例（~14KB），包含完整数据集。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，使用 `highcharts-vue` 的 `Chart` 渲染 `chart.type: 'scatter'`，`zoomType: 'xy'` 启用框选缩放；含 JNPF 免责声明。 |

## For AI Agents

### Working in this directory
- 大量内联数据点，避免无意义的 reformat —— diff 噪音大。
- 通过 `series[*].marker` 控制散点形状；`tooltip.headerFormat = '<b>{series.name}</b><br>'`、`pointFormat: '{point.x} cm, {point.y} kg'`。
- 修改坐标轴单位时同步调整 tooltip。

### Common patterns
- `legend.enabled: true` + 双 series（男 / 女）；`accessibility.enabled: false`。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`
