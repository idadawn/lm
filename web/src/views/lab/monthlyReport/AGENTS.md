<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# monthlyReport

## Purpose
"月度质量统计报表"页面：按日期区间/班次/班别/产品规格筛选，左侧明细表 + 右侧班组统计 + 顶部汇总卡片，支持刷新与导出。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 报表主页：标题/导出 + SummaryCards + FilterPanel + 明细表 + 班组面板 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 汇总卡、筛选面板、明细表、班组面板、图表与配置弹窗 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `qualifiedColumns`/`unqualifiedColumns` 来自 `reportConfigs`，决定明细/汇总展示哪些列；与 `reportConfig` 模块强相关。
- 导出走后端 Excel/PDF 接口，注意鉴权与文件流处理。

### Common patterns
- 双向绑定多个筛选 `v-model:dateRange` 等到 `FilterPanel`。
- `currentDateRange` 文案根据 `dateRange` 派生。

## Dependencies
### Internal
- `/@/api/lab/monthlyReport`, `/@/api/lab/reportConfig`
- `/@/components/Icon`
### External
- `ant-design-vue`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
