<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsPie

## Purpose
Highcharts 饼图（Pie）演示页面，包含半圆 / 钻取等扩展形态（~6KB 配置），属于 `extend/graphDemo` 第三方组件示例集。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，使用 `highcharts-vue` 的 `Chart` 渲染 `chart.type: 'pie'`，含 dataLabels 与中文图例；带 JNPF 免责声明。 |

## For AI Agents

### Working in this directory
- 半圆通过 `plotOptions.pie.startAngle: -90` + `endAngle: 90` 实现。
- 钻取（drilldown）需要单独引入 `highcharts/modules/drilldown` 并注册；当前文件如使用钻取请确认模块已加载。
- 保留免责声明 alert。

### Common patterns
- `dataLabels.enabled: true` + `format: '<b>{point.name}</b>: {point.percentage:.1f}%'`。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`（视情况依赖 `highcharts/modules/drilldown`）
