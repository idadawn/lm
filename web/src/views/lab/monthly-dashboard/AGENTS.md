<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# monthly-dashboard

## Purpose
"月度生产驾驶舱"页面：聚合一段时间（默认按月）的产量、合格率、班次对比、层压趋势、不合格 Top5 等指标。kebab-case 路径与 `dashboard`（实时驾驶舱）并列。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 月度驾驶舱主页：日期区间选择 + 多张图表 + 班次对比 + 明细表 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | KPI 卡片、各种 ECharts 图表、ChatAssistant 等 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 父子组件通过 ref 触发刷新；图表组件统一接 `data` + `loading` props，不在内部请求。
- `dailyProduction` 派生于 `data` 的某字段，注意空数组兜底。
- 路径含连字符 `monthly-dashboard`，路由 name 与 import 别名注意保持一致。

### Common patterns
- `animate-card`/`animate-row delay-N` 进入动画类。
- `a-alert` 在 `summary.totalWeight === 0` 时提示空数据。

## Dependencies
### Internal
- `/@/api/lab/monthly-dashboard` 或类似命名空间
- `/@/components/Icon`
### External
- `ant-design-vue`, `dayjs`, `echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
