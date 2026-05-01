<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsBarAcross

## Purpose
ECharts 横向条形对比图示例。以「世界人口总量（巴西/印尼/美国/印度/中国/世界人口）」为案例展示 2017 与 2018 两年数据并列对比的横向柱图。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `xAxis: type=value` + `yAxis: type=category` 的横向条形示例配置 |

## For AI Agents

### Working in this directory
- 横向条形关键在 `xAxis.type='value'` 与 `yAxis.type='category'` 互换；不要保留默认竖向，否则示例失真。
- 类目顺序与 `series.data` 数组下标对应，新增项需双侧同步。
- `legend.data` 与 `series.name` 必须一致，否则图例不展示。

### Common patterns
- `grid` 用百分比 padding 自适应容器。
- `tooltip.trigger='axis'` + `axisPointer.type='shadow'` 提供整列高亮 hover。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
