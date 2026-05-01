<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsLineArea

## Purpose
ECharts 折线 + 面积图（Line/Area）演示页面，用于展示填充区域折线的基础配置。属于 `extend/graphDemo` 图表示例。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，使用 `/@/components/Chart` 渲染折线 + areaStyle 区域；`reactive` 静态数据；`defineOptions({ name: 'extend-graphDemo-echartsLineArea' })`。 |

## For AI Agents

### Working in this directory
- 调整 `series[*].areaStyle` 控制是否填充；多 series 时通过 `stack` 字段做堆叠。
- 不要在此引入业务 API；如需动态数据，请抽到 hooks 中。
- 路由命名约定保持 `extend-graphDemo-echartsLineArea`。

### Common patterns
- 标题 / x 轴 / y 轴 / legend 全部中文，符合本项目本地化要求。
- 与同目录其他 `echarts*` demo 共享 `Chart` 包装组件与外层 `page-content-wrapper` 容器样式。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`、`vue` reactive
