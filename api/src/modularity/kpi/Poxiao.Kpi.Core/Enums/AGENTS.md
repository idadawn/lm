<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
KPI 模块业务枚举。所有枚举 JSON 序列化均按 name 输出（`EnumUseNameConverter`），并带 `[Description]` 中文文案，DB 落库使用 `EnumToStringConvert`。

## Key Files
| File | Description |
|------|-------------|
| `MetricType.cs` | `MetricType` (基础/派生/复合)；同文件包含 `MetricDisplayMode`、`MetricFilterModel`、`DeriveType`(PTD/POP/Cumulative/Moving/Difference/DifferenceRatio/TotalRatio/Ranking)、`RankingType`、`TrendType`(Up/Down)、`MetricGradeType`(Value/Rang) |
| `DataModelType.cs` | `DataModelType`(Db/Model)、`SchemaStorageType`(Table/View/RealTime)、`MetricDataType`(Static/RealTime) |
| `GranularityType.cs` | 时间粒度(Day/Week/Month/Quarter/Year/Hour/Minute/Second) + `DisplayOption`(All/Latest) |
| `GotType.cs` | 思维图类型(Cov/Dash) |
| `StorageFqType.cs` | 存储频率(Second/Minute/Hour/Day/Week) |
| `CovRuleValueType.cs` | 价值链规则值类型(Value/Percent) + `CovRuleOperatorsType`(GreaterThan/Between/LessThan) |
| `MetricNoticeType.cs` | 消息提示类型(Node/Rule) |
| `AnalysisStatus.cs` | 分析任务状态(NotStarted/InProgress/Completed/Canceled/Failed) |

## For AI Agents

### Working in this directory
- 枚举值的整数取值是 API 兼容契约，不要随意调整顺序；新增成员追加到末尾。
- 每个 enum 必须带 `[JsonConverter(typeof(EnumUseNameConverter<T>))]` 与每个成员的 `[Description("中文")]`，否则前端文案会丢失。
- 多 enum 同文件出现属常态（如 `MetricType.cs`），新增请就近放置。

### Common patterns
- 数据库列声明：`SqlParameterDbType = typeof(EnumToStringConvert)`。
- UI 取描述：`enumValue.GetDescription()`。

## Dependencies
### Internal
- `Poxiao.Common`(`EnumUseNameConverter`、`EnumToStringConvert`、`GetDescription`)
### External
- Newtonsoft.Json, System.ComponentModel

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
