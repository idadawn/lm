<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricIGrade

## Purpose
指标分级服务。每个指标可定义若干分级（按值或区间），返回时会带颜色与状态文案，被 `MetricDataService` 引用以渲染达成情况。

## Key Files
| File | Description |
|------|-------------|
| `IMetricGradedService.cs` | 标准 CRUD + 按 metricId 列表 |
| `MetricGradedService.cs` | 实现：`Value` 字段为 `decimal`，输出时 `ToString()`；列表按创建时间 + 状态联合排序 |

## For AI Agents

### Working in this directory
- 分级类型 `MetricGradeType.Value/Rang`，区间方向由 `CovRuleValueType.Value/Percent` 与 `TrendType.Up/Down` 控制。
- `Status` 与 `StatusColor` 一般引用 `MetricCovStatusEntity` 中的字典项，便于全局统一。

### Common patterns
- 服务直接返回 `List<MetricGradedListOutput>`，无分页（按指标维度查询通常数量小）。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricGraded`、`Enums/CovRuleValueType`/`MetricGradeType`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
