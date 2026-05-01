<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# graphDemo

## Purpose
图表示例集合。集中演示项目 `/@/components/Chart`（基于 ECharts 封装）以及 Highcharts 各类常见图表，作为业务统计 / BI 仪表盘开发的代码片段参考。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `echartsBar/` | 堆叠柱状图（含工具箱与魔法切换） |
| `echartsBarAcross/` | 横向条形对比图 |
| `echartsCandlestick/` | K 线 / 蜡烛图（带时间序列与上下色） |
| `echartsFunnel/`, `echartsGauge/`, `echartsLineArea/`, `echartsLineBar/`, `echartsPie/`, `echartsScatter/`, `echartsTree/` | 漏斗 / 仪表盘 / 折线面积 / 折柱混合 / 饼图 / 散点 / 树图示例 |
| `highchartsArea/`, `highchartsBellcurve/`, `highchartsBullet/`, `highchartsColumn/`, `highchartsFunnel/`, `highchartsGauge/`, `highchartsLine/`, `highchartsPie/`, `highchartsScatter/`, `highchartsWordcloud/` | Highcharts 系列示例 |

## For AI Agents

### Working in this directory
- ECharts 示例统一使用项目内 `Chart` 组件（`/@/components/Chart`），不要直接 `echarts.init`。
- 配置以 `reactive(options)` 方式声明，便于响应式刷新；动态数据源时把 `options` 内的 `series.data` 替换为 ref。
- Highcharts 与 ECharts 不要混用同一个图表实例，分子目录隔离。

### Common patterns
- 命名 `extend-graphDemo-<chart>`；高度通过 prop `height="500px"` 显式声明。
- 数据均为前端 mock，业务化需替换为 `/@/api/*` 接口。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`, `highcharts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
