<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsBar

## Purpose
ECharts 堆叠柱状图示例。展示「直接访问 / 邮件营销 / 联盟广告 / 视频广告 / 搜索引擎」多系列堆叠形态，含工具箱（数据视图、图表类型切换、还原、保存图片）。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件图表 demo：`<Chart :options height="500px" />`，配置 `tooltip / toolbox / legend / xAxis / yAxis / series` |

## For AI Agents

### Working in this directory
- `toolbox.feature.magicType.type` 含 `line / bar / stack / tiled`，去掉某项前确认业务允许图表切换。
- 多系列堆叠依赖 `series[i].stack` 同名分组；新增系列时同步 `legend.data`。
- 该 demo 数据为静态数组，业务化请把 `series.data` 改为 ref 并按接口刷新。

### Common patterns
- `defineOptions({ name: 'extend-graphDemo-echartsBar' })`；统一外壳 `page-content-wrapper` + `bg-white`。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
