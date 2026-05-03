<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# analyticalTools

## Purpose
统计分析工具集页面：用 Tabs 聚合三类图表分析 —— 正态分布（NormalDistributionChart）、SPC 控制图（SpcChart：单值移动极差/均值标准差/均值极差/中位数极差）、KPI 多指标对比（KpiChart）。基于 `useECharts` 渲染，数学计算用 `mathjs`，可直接对接检测数据进行过程能力分析。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tabs 容器，三 tab 切换三个 chart 子组件。 |
| `NormalDistributionChart.vue` | 正态分布上传/规格上下限筛选 + 偏度峰度计算。 |
| `SpcChart.vue` | SPC 表单（日期/材料/温度/时间/图形类型）+ 4 种 SPC 控制图渲染。 |
| `KpiChart.vue` | 多选指标 + 维度 → 调 `defHttp` 拉数据 → ECharts 多系列对比。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `config/` | 图表常量、类型、计算工具与 mock 数据 (see `config/AGENTS.md`) |

## For AI Agents

### Working in this directory
- ECharts 实例通过 `useECharts(chartRef)` 复用；销毁/重渲依赖 hook 内部，不要手动 dispose。
- SPC 系数（A2/A3/B3/B4/D3/D4/E2）来自 `config/const.ts`，按样本数 n 索引（n=2 起 index=0），改动会影响所有 SPC 图。
- 图表数据流：组件内 form → `onSearch` → 调 `config/utils.ts` 中的 `normalDistributionMethod` / SPC 计算函数 → 输出 `MarkLineData` 与系列。

### Common patterns
- `defineOptions({ name: 'AnalyticalTools' })`
- mathjs 函数式（`mean`, `std`, `variance` 等）

## Dependencies
### Internal
- `/@/hooks/web/useECharts`, `/@/utils/http/axios`, `./config`
### External
- `echarts`, `mathjs`, `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
