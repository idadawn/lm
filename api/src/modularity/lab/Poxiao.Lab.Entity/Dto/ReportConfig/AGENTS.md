<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ReportConfig

## Purpose
月度质量报表（及类似聚合报表）"列配置"的 DTO。每条记录定义一个统计列：等级名集合、是否表头/百分比/显示/显示比率，以及关联到哪条 JUDGE 公式。

## Key Files
| File | Description |
|------|-------------|
| `ReportConfigDto.cs` | `Id/Name/LevelNames(List<string>)/IsSystem/SortOrder/Description/IsHeader/IsPercentage/IsShowInReport/IsShowRatio/FormulaId`。 |
| `ReportConfigInputDto.cs` | 创建/更新输入（同结构，去 `Id`/`IsSystem`）。 |

## For AI Agents

### Working in this directory
- `IsSystem=true` 表示系统内置列，前端禁止用户删改；新建用户列时强制 `IsSystem=false`。
- `LevelNames` 是判定等级名称数组：服务端按 `Name in LevelNames` 在 `LAB_INTERMEDIATE_DATA_JUDGMENT_LEVEL` 中匹配并聚合，因此**等级重命名时必须同步更新所有 ReportConfig**，否则统计会丢失。
- `FormulaId` 限定该列只统计某条 JUDGE 公式的等级（多公式并存时关键）。
- `IsShowRatio` 仅在 `IsPercentage=true` 时生效。

### Common patterns
- 报表列配置驱动 `MonthlyQualityReportService` 的动态列布局，是把 JUDGE 等级映射到表格列的桥梁。

## Dependencies
### Internal
- `../MonthlyQualityReport/`（消费方）、`../IntermediateDataJudgmentLevel/`、`../IntermediateDataFormula/`。
- 服务实现位于 `Poxiao.Lab/Service/ReportConfigService.cs`。

### External
- 无。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
