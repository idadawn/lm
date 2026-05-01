<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsScatter

## Purpose
ECharts 散点图（Scatter）演示页面，结合 `echarts-stat` 计算线性回归并叠加趋势线。属于 `extend/graphDemo` 图表示例集。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，使用 `ecStat.regression` 生成回归曲线，并以双 series（散点 + line）渲染；命名 `extend-graphDemo-echartsScatter`。 |

## For AI Agents

### Working in this directory
- `ecStat` 用于统计计算；如要切换回归类型（linear / polynomial / exponential），改 `ecStat.regression('linear', data)`。
- 数据为内联静态二维数组；接入实时数据时请通过 props/emit 注入而非直接改文件。
- 命名空间保持与路由一致。

### Common patterns
- 同时使用 `series.type: 'scatter'` 与 `'line'`，并通过 `markPoint` 标注点。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`、`echarts-stat`、`vue` reactive
