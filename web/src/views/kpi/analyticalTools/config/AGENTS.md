<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# config

## Purpose
`analyticalTools` 的纯逻辑层：常量（SPC 系数表、图表类型、列名、控制线样式）、TypeScript 类型（`DataSet` / `MarkLineData` / `SpcFormState` 等）、计算工具（正态分布、SPC 控制图均值/极差/标准差），以及一组 mock 数据用于联调或离线演示。

## Key Files
| File | Description |
|------|-------------|
| `const.ts` | `DataSetKeyEnum`、`ChartTypeEnum`、`SpcChartKey`、SPC A2/A3/B3/B4/D2/D3/D4/E2 系数表、`MarkLineDataLineStyle`。 |
| `types.ts` | `DataSet`、`SpcFormState`、`DataItem`、`SpcLineType`、`MarkLineData`。 |
| `utils.ts` | `normalDistributionMethod` 等基于 `mathjs` 的统计计算函数。 |
| `mock.ts` | 大量 SPC/正态分布演示数据集。 |
| `Normal-distribution.svg` | 页面静态图示。 |

## For AI Agents

### Working in this directory
- 系数表索引语义：`SpcA2[0]` 对应样本数 n=2，依此类推；扩展到更大样本时务必查 SPC 标准表，而非外推。
- `Reservedfixed = 4` 是统一的小数保留位数，调整会改变所有展示精度。
- 计算函数纯函数化、不依赖 Vue 上下文，便于单测；保持这一约定。

### Common patterns
- 基于 `mathjs` 的 `mean / std / variance / median` 等函数式调用
- 配套 `JSDoc` 中文注释解释每个枚举/系数的含义

## Dependencies
### External
- `mathjs`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
