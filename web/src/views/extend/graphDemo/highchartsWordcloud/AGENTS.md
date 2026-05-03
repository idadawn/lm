<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# highchartsWordcloud

## Purpose
Highcharts 词云（Wordcloud）演示页面，需要注册 `highcharts/modules/wordcloud` 模块。属于 `extend/graphDemo` 第三方组件示例集。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件页面，注册 wordcloud 模块，将一段英文文本拆词后按频率作为 series 数据渲染词云；含 JNPF 免责声明。 |

## For AI Agents

### Working in this directory
- 必须 `highchartsWordcloud(Highcharts)` 注册模块；否则 series.type='wordcloud' 不可用。
- 词频生成是即时计算（`text.split(/[,\. ]+/g).reduce`），更换文本会自动刷新。
- 保留免责声明 alert。

### Common patterns
- 数据结构 `{ name, weight }[]`；`series[0].rotation` 控制词条旋转角度。

## Dependencies
### Internal
- `page-content-wrapper` 全局样式
### External
- `highcharts`、`highcharts-vue`、`highcharts/modules/wordcloud`
