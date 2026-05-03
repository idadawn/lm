<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsGauge

## Purpose
ECharts 仪表盘（Gauge）演示页面，模拟车速表样式，包含坐标轴线、刻度、分隔线、标题等完整配置。隶属于 `extend/graphDemo` 图表示例集合。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 仪表盘单文件页面，使用 `onMounted/onUnmounted` 配合 `setInterval` 演示动态更新 series 数据，`Chart` 组件来自 `/@/components/Chart`。 |

## For AI Agents

### Working in this directory
- 该示例存在定时器，修改时记得在 `onUnmounted` 中清理，避免泄漏。
- 调整刻度颜色 / 范围请改 `axisLine.lineStyle` 与 `min/max/splitNumber`。
- 路由命名 `extend-graphDemo-echartsGauge` 必须与文件路径保持一致。

### Common patterns
- `reactive` 配置对象 + 单一 `series[0]` 仪表盘项；通过 `data: [{ value, name }]` 控制指针。
- 中文 tooltip / 标签格式化字符串 `{a} <br/>{c} {b}`。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`、`vue` reactive 与生命周期 hooks
