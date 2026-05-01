<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsBellcurve

## Purpose
Highcharts 贝尔曲线（直方图 + 正态分布曲线）演示页面，使用 `highcharts/modules/histogram-bellcurve` 模块。属于 `extend/graphDemo` 第三方组件示例集。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，注册 histogram-bellcurve 模块后渲染散点 / 直方图 / 钟形曲线三 series；含 JNPF 第三方组件免责声明。 |

## For AI Agents

### Working in this directory
- 注意必须先 `import highchartsHistogramBellcurve from 'highcharts/modules/histogram-bellcurve'` 并 `highchartsHistogramBellcurve(Highcharts)` 注册，再使用 bellcurve series。
- 双 xAxis / 双 yAxis：原始数据用主轴，钟形曲线用 `opposite: true` 副轴。
- 不要去掉免责声明 `<a-alert>`。

### Common patterns
- `import Highcharts from 'highcharts'` + 模块注册 + `import { Chart } from 'highcharts-vue'`。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`、`highcharts/modules/histogram-bellcurve`
