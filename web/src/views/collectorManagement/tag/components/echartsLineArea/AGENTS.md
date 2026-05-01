<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# echartsLineArea

## Purpose
Tag history curve modal body. Two-tab view: 曲线 (ECharts line via `/@/components/Chart`) and 表格 (`BasicTable` over `historyPage`), both filtered by a `RangePicker` (default range comes from the `dataEchart.date` prop).

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Form (date range) + tabs; switches between `<Chart :options height="500px">` and `BasicTable`; reloads on search/reset, calls `tagHistory` for the chart series |

## For AI Agents

### Working in this directory
- `dataEchart.id` is the tag id and `dataEchart.date` the default `[start, end]` array. Mutating these props directly will leak back to the parent — clone if you need to.
- The chart's `options` is built locally; preserve `formatter` and `axisPointer` defaults expected by ECharts.
- `valueFormat="YYYY/MM/DD HH:mm:ss"` (slashes) must match `historyTable/index.vue` and the backend.

### Common patterns
- `Chart` is the project's ECharts wrapper — pass `:options` and `height`.

## Dependencies
### Internal
- `/@/api/collector` (`tagHistory`, `historyPage`), `/@/components/{Chart,Table}`
### External
- `ant-design-vue`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
