<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsArea

## Purpose
Highcharts 面积图（Area）演示页面，含负值演示。`extend/graphDemo` 中 highcharts 系列示例之一，展示如何在项目中嵌入第三方 Highcharts 组件（含免责声明 alert）。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，从 `highcharts-vue` 导入 `Chart`，渲染苹果/橘子/梨/葡萄/香蕉的面积折线对比；含 `<a-alert>` 免责声明。 |

## For AI Agents

### Working in this directory
- 必须保留顶部 “Highcharts 不属于 JNPF 产品 …” 的 a-alert 免责声明（产品合规要求）。
- 调整图表只改 `options`；`accessibility: { enabled: false }` 与 `credits.enabled: false` 是项目默认。
- 不要在此引入 echarts —— 文件目的就是 Highcharts 示例；与同级 `echarts*` 示例保持互不依赖。

### Common patterns
- `import { Chart } from 'highcharts-vue';` + `<Chart :options="options" class="h-500px" />`。

## Dependencies
### Internal
- 全局样式 `page-content-wrapper` 及 ant-design-vue `a-alert`
### External
- `highcharts-vue`、`highcharts`、`vue` reactive
