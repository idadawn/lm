<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MonthlyQualityReport

## Purpose
月度质量报表 DTO 集合。覆盖顶部汇总、明细、班组、质量趋势、不合格分类、班次对比、动态列定义等多份子表，由 `IMonthlyQualityReportService` (`Poxiao.Lab/Service/IMonthlyQualityReportService.cs`) 提供。

## Key Files
| File | Description |
|------|-------------|
| `MonthlyQualityReportDto.cs` | 入口查询：`MonthlyQualityReportQueryDto(StartDate/EndDate/Shift甲乙丙/ShiftNo/ProductSpecCode/LineNo)` + `QualityTrendDto(Date/QualifiedRate/QualifiedCategories Dict<level,%>)`。 |
| `MonthlyQualityReportSummaryDto.cs` | 顶部 KPI 汇总（总产量/合格率/不合格率/分级占比）。 |
| `MonthlyQualityReportDetailDto.cs` | 明细表格行：按炉号或日期聚合的指标。 |
| `MonthlyQualityReportShiftGroupDto.cs` | 班组（甲/乙/丙）聚合数据。 |
| `MonthlyQualityReportResponseDto.cs` | `GetReportAsync` 一次返回的完整组合（summary/details/shift/trend/categories/columns）。 |
| `LevelStatDto.cs` | 单个判定等级的数量/占比统计单元。 |
| `JudgmentLevelColumnDto.cs` | 动态列定义（合格/不合格等级列）DTO，由 `GetColumnsAsync` 返回。 |

## For AI Agents

### Working in this directory
- 等级列**是动态的**：合格/不合格列名由 `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` 表运行期决定，不要硬编码列名；前端按 `JudgmentLevelColumnDto` 渲染表头。
- `Shift` 必须接受中文字面量"甲/乙/丙"；与 `RawDataEntity.Shift` 解析保持一致（炉号格式 `[产线][班次汉字][8位日期]`）。
- 趋势的 `QualifiedCategories` 是 `Dictionary<string, decimal>`：键是合格等级名（如"优级/合格/特采"），值是百分比，前端按 `QualityStatusEnum=Qualified` 的等级渲染堆叠图。

### Common patterns
- 大多数 DTO 提供 `decimal` 百分率（已 ×100），不要在前端再乘 100。

## Dependencies
### Internal
- `../../Enum/QualityStatusEnum.cs`、`../ReportConfig/ReportConfigDto.cs`、`../IntermediateDataJudgmentLevel/`。

### External
- 无（纯 POCO）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
