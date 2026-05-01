<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsColumn

## Purpose
Highcharts 柱状图（Column）演示页面，是 `extend/graphDemo` 第三方组件示例集成员，展示分组柱状对比。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，从 `highcharts-vue` 渲染 `chart.type: 'column'`，配合 `plotOptions.column` 控制柱宽与间距；含 JNPF 第三方组件免责声明 alert。 |

## For AI Agents

### Working in this directory
- 保留顶部免责声明 `<a-alert>`。
- 与 `highchartsArea` 几乎对称，仅 `chart.type` 与 series 配置不同；改动时保持目录间风格一致。
- 数据为静态演示数据，不要接入业务 API。

### Common patterns
- `accessibility.enabled: false`、`credits.enabled: false` 默认关闭；类目轴 `xAxis.categories` 中文。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`
