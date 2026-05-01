<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsGauge

## Purpose
Highcharts 仪表盘（Gauge）演示页面，需要 `highcharts/highcharts-more` 与 `highcharts/modules/solid-gauge` 模块支持。属于 `extend/graphDemo` 第三方组件示例。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，注册 highcharts-more 与 solid-gauge 模块后渲染速度 / 转速 / 油耗等多 pane 仪表盘；含免责声明 alert。 |

## For AI Agents

### Working in this directory
- 注册顺序：`highchartsMore(Highcharts)` 必须在 `solidGauge(Highcharts)` 之前。
- 使用 `pane: { startAngle, endAngle, background }` 控制仪表盘外观。
- 修改区间颜色请改 `yAxis.plotBands`。

### Common patterns
- `chart.type: 'gauge' | 'solidgauge'`；`tooltip.enabled: false` 在仪表盘中常见。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`、`highcharts/highcharts-more`、`highcharts/modules/solid-gauge`
