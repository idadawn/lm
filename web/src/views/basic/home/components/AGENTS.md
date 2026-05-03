<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Page-local widgets used by the home dashboard. A mix of ECharts cards (Visit*, Site*, Sales*), the warning-list `MessageDrawer`, the `homeModal` filter form, and supporting modal components. Most charts accept a `loading` prop and render via `/@/components/Chart`.

## Key Files
| File | Description |
|------|-------------|
| `MessageDrawer.vue` | 预警列表 drawer — `BasicDrawer` with mock items, opens `homeModal.vue` on click |
| `SiteAnalysis.vue` | Multi-tab analysis card; tab1 shows `homeChartsManageMent` + `VisitAnalysis` + `VisitAxis` + `VisitGauge`/`Source`/`Pie`, tab2/3/4 swap views |
| `homeModal.vue` | Form wrapper for filtering / drilling into chart points |
| `homeComponents/` | Chart batch loader subdir (see `homeComponents/AGENTS.md`) |
| `optimalManagement/` | Sub-feature: 寻优管理 chart group (see `optimalManagement/AGENTS.md`) |
| `monitorManagement.vue` | Tab4 monitoring view |
| `errorDetailMsgModal.vue` | Modal showing detailed error / alarm message |
| `VisitAxis.vue` / `VisitAxisMonitor.vue` | Stacked-line / monitor variants |
| `VisitPie.vue` / `visitThreePie.vue` / `VisitPieMonitor.vue` | Pie chart variants |
| `VisitGauge.vue` / `VisitRadar.vue` / `VisitSource.vue` / `VisitCategory.vue` / `VisitdoubleCategory.vue` | ECharts gauge/radar/category cards |
| `VisitAnalysis.vue` / `VisitAnalysisBar.vue` | Multi-metric panels (tab2 uses `Bar`) |
| `GrowCard.vue` / `SalesProductPie.vue` | Original demo cards (referenced from `OriginalDefault.vue`) |
| `props.ts` | Tiny prop-types helper used by chart components |

## For AI Agents

### Working in this directory
- Chart components are presentational — they should not call APIs directly. Data is pulled by `homeComponents/index.vue` (collects via `getHomeChartsDataList`) or by `optimalManagement` (via `getChartsDataList`) and forwarded as props.
- `loading` is a passthrough into `<Chart :loading>` for the spin overlay.
- Naming: `Visit*` is legacy demo (从 vben 模板沿用), kept because routes/components reference them; do not rename.

### Common patterns
- Each card wraps a `<Card>` with a fixed-height `<Chart :options="...">`.
- Mock placeholder text `****` is intentional — replaced at runtime by `TitletextName` from API.

## Dependencies
### Internal
- `/@/components/Chart`, `/@/components/Drawer`, `/@/components/Modal`, `/@/api/system/message`, `/@/api/basic/charts`
### External
- `ant-design-vue`, `@ant-design/icons-vue`, `echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
