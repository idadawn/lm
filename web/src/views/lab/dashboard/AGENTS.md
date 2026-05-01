<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dashboard

## Purpose
"生产驾驶舱"主页：实时监控生产质量与设备状态。由顶部日期区间 + KPI 卡片 + 多张 ECharts 图表（合格分布、层压趋势、缺陷 Top5、生产热力、厚度相关性）组成。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 驾驶舱主页：日期 RangePicker + 刷新 + 图表网格布局 |
| `README.md` | 该模块说明（保留，作者文档）|

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | KPI 卡片、各种图表与 AI 助手 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 所有图表组件统一接收 `start-date` / `end-date`（'YYYY-MM-DD'）字符串 props。
- `refreshData()` 通过子组件 ref 调用其 `refresh`/`reload` 方法。
- AI 助手已在主页移除（注释保留），新增请重新挂载到 `index.vue`。

### Common patterns
- `dayjs` 处理日期；`Icon` 来自 `/@/components/Icon`。
- `chart-row / chart-col-2 / chart-col-3` 自定义网格类。

## Dependencies
### Internal
- `/@/components/Icon`, `/@/hooks/web/useMessage`
- 各子图表 API（通常 `/@/api/lab/dashboard`）
### External
- `dayjs`, `ant-design-vue`, `echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
