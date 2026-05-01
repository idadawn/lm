<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsCandlestick

## Purpose
ECharts K 线 / 蜡烛图示例。基于内置时序数据演示开盘 / 收盘 / 最低 / 最高四值的蜡烛图绘制方式，包含上涨（红 `#ec0000`）与下跌（绿 `#00da3c`）双色配置。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 蜡烛图配置 + `splitData` 工具拆分日期与 OHLC 数据 |

## For AI Agents

### Working in this directory
- 颜色方案遵循 A 股惯例（涨红跌绿），不要用国际市场配色（绿涨红跌）覆盖。
- 数据每条 5 元素：`[日期, open, close, lowest, highest]`，缺一会导致 `splitData` 报错。
- `splitData` 是模块内函数，业务化抽到公共工具时连同色值常量一起迁移。

### Common patterns
- 大数据量时叠加 `dataZoom` 可缩放 X 轴；本示例未启用，需要时按 ECharts 文档补齐。

## Dependencies
### Internal
- `/@/components/Chart`
### External
- `echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
