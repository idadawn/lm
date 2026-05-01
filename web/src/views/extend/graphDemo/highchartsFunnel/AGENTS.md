<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsFunnel

## Purpose
Highcharts 漏斗图（Funnel）演示页面，需要注册 `highcharts/modules/funnel` 模块。属于 `extend/graphDemo` 第三方组件示例，与 `echartsFunnel` 形成对比演示。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，注册 funnel 模块后渲染单 series 漏斗；含免责声明 alert；命名 `extend-graphDemo-highchartsFunnel`。 |

## For AI Agents

### Working in this directory
- 必须 `highchartsFunnel(Highcharts)` 注册模块；忘记会运行时报错。
- 与 echartsFunnel 在路由层面是两个独立菜单，不要混用配置结构。
- 保留免责声明 `<a-alert>`。

### Common patterns
- 单 series 漏斗 + dataLabels 中文标签；`accessibility.enabled: false`。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`、`highcharts/modules/funnel`
