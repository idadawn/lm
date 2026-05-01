<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsLineBar

## Purpose
ECharts 折线 + 柱状混合图（Line + Bar）演示页面，演示双 series 混合渲染与共享坐标轴。隶属 `extend/graphDemo` 图表示例集。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，使用 `/@/components/Chart` 渲染折线 + 柱状双 series；命名 `extend-graphDemo-echartsLineBar`。 |

## For AI Agents

### Working in this directory
- 不同类型 series 通过 `type: 'bar' | 'line'` 区分；要联动 y 轴时按需配置多 yAxis。
- 仅作为前端演示，无后端依赖；不要在此添加 API 请求。
- 保持文件单一 `index.vue` 的结构，与其它 echarts* demo 一致。

### Common patterns
- `reactive(options)` + `<Chart :options="options" height="500px" />`。
- 中文 legend 与 tooltip 格式化。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`、`vue` reactive
