<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# prediction

## Purpose
预测管理 - 仪表盘视图：组合 ECharts 图表的总览页面。当前与 `views/prediction/overview/` 内容几乎完全重复（站点分析、雷达、仪表盘、品类占比等），疑似复制粘贴待整合。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 容器：`SiteAnalysis` + `VisitRadar` + `VisitGauge` + `SalesProductPie` 三栏组合 |
| `props.ts` | 共享 `BasicProps`（width/height 默认 `100%`/`280px`） |
| `SiteAnalysis.vue` / `VisitRadar.vue` / `VisitGauge.vue` / `SalesProductPie.vue` | 主用 4 张图 |
| `VisitAnalysis.vue` / `VisitAnalysisBar.vue` / `VisitAxis.vue` | 备用图表 |

## For AI Agents

### Working in this directory
- **去重**：本目录与 `views/prediction/overview/` 内容几乎一致，重构时考虑抽到 `components/` 或 `views/prediction/overview/` 复用，避免双向修改。
- 数据目前为模拟（`setTimeout(()=>loading=false, 500)`），接入真实接口时同时更新两处。

### Common patterns
- `useECharts` 渲染、`Card` 包装。

## Dependencies
### Internal
- `/@/hooks/web/useECharts`
### External
- `ant-design-vue`、`echarts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
