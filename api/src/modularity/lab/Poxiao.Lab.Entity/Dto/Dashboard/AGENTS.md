<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dashboard

## Purpose
驾驶舱（Dashboard）页面的查询/输出 DTO：包含 KPI、质量分布、叠片系数趋势、缺陷 Top、生产热力图、厚度-叠片关联、今日产量等图表数据契约。被 `IDashboardService` / `DashboardService` 消费。

## Key Files
| File | Description |
|------|-------------|
| `DashboardQueryDto.cs` | 通用查询入参（日期范围/产线等）。 |
| `DashboardKpiDto.cs` | KPI 数据：`TotalWeight/QualifiedRate/LaminationFactorAvg/LaminationFactorTrend/Warnings`，`DashboardWarningDto` 包含 `Type(quality/process/device)`、`Level(info/warning/error)`。 |
| `QualityDistributionDto.cs` | 质量等级分布占比。 |
| `LaminationTrendDto.cs` | 叠片系数日/月趋势。 |
| `DefectTopDto.cs` | 缺陷 Top5 排行。 |
| `ProductionHeatmapDto.cs` | 生产时间×产线热力图。 |
| `ThicknessCorrelationDto.cs` | 厚度 vs 叠片系数散点关联。 |
| `DailyProductionDto.cs` | 今日产量与昨日对比。 |

## For AI Agents

### Working in this directory
- 这些 DTO 只承载图表数据；时间窗、聚合计算放在 `Poxiao.Lab/Service/DashboardService.cs`。
- 新增图表需同时新增 DTO 和 `IDashboardService` 上的方法，并在 `Poxiao.Lab.Service` 项目中再镜像一份接口（项目里存在两份 `IDashboardService`，是历史结构）。
- `DashboardWarningDto.Type/Level` 是字符串枚举（不要改成 enum），前端按字符串渲染图标。

### Common patterns
- 所有列表初始化为 `new()`，避免序列化输出 `null`。

## Dependencies
### Internal
- 被 `../../Poxiao.Lab.Service/IDashboardService.cs` 与 `../../Poxiao.Lab/Service/IDashboardService.cs`、`DashboardService.cs` 引用。

### External
- 无（纯 POCO）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
