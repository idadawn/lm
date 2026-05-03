<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsPie

## Purpose
ECharts 饼图（Pie）演示页面，展示分类占比与 emphasis 高亮、百分比 tooltip。属于 `extend/graphDemo` 图表示例集。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，`reactive` options + `Chart` 组件渲染；`defineOptions({ name: 'extend-graphDemo-echartsPie' })`。 |

## For AI Agents

### Working in this directory
- 调整 `series[*].radius` 可在普通饼图与圆环图之间切换；`roseType: 'radius' | 'area'` 切换玫瑰图。
- legend 与 data 的 `name` 字段必须一一对应才能正确高亮。
- 数据为静态演示数据，不要在此处接入业务 API。

### Common patterns
- tooltip `formatter: '{a} <br/>{b} : {c} ({d}%)'`、emphasis 阴影；与其它 echarts demo 保持外层 `page-content-wrapper` 结构。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`、`vue` reactive
