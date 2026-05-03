<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsLine

## Purpose
Highcharts 折线图（Line）演示页面，属于 `extend/graphDemo` 第三方组件示例集，演示多 series 折线与中文 legend。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，使用 `highcharts-vue` 的 `Chart` 渲染 `chart.type: 'line'`；含免责声明 alert；命名 `extend-graphDemo-highchartsLine`。 |

## For AI Agents

### Working in this directory
- 调整线型：`series[*].dashStyle: 'Solid' | 'Dash' | 'Dot'`；标记点 `marker.symbol`。
- 无业务依赖；保留免责声明。
- 与 `echartsLineArea` 在路由层独立，但视觉风格保持一致。

### Common patterns
- 折线 + 数据点 marker；标题 + 副标题 + 中文 categories。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`
