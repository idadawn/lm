<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Models

## Purpose
实验室模块的非数据库 POCO 模型。当前仅一个核心模型 `FurnaceNo`（炉号封装类）：负责按正则解析整个流水线最关键的标识——炉号字符串。

## Key Files
| File | Description |
|------|-------------|
| `FurnaceNo.cs` | 解析 `[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号][W/w?][特性汉字?]` 格式（示例：`1甲20251101-1-4-1W脆`），暴露 `LineNo/Shift/ProdDate/FurnaceBatchNo/CoilNo/SubcoilNo/SpecialMarker/FeatureSuffix/LineNoNumeric`，含控制字符与尾部噪声清洗的 `Regex`，并提供格式化输出（去特性后缀）以构建 `FurnaceNoFormatted`。 |

## For AI Agents

### Working in this directory
- `FurnaceNo` 是导入与计算流水线的事实标准：`RawDataEntity.FurnaceNo` 存原文，`IntermediateData.FurnaceNoFormatted` 存重构后的标准化版本（不含特性描述）；磁性数据回填靠 `FurnaceNoFormatted` 关联。
- 修改正则前先看 `RawDataImportSessionService` 与 `MagneticDataImportSessionService` 的炉号匹配单测/调用点；放宽正则可能导致历史数据无法重新解析。
- 不要把 SqlSugar 实体或 IO 放在本目录；保持纯 POCO + 静态 Regex。
- 解析失败时让属性返回 `null`（构造函数不抛异常），上层由 `RawDataValidationService` 决定如何处理。

### Common patterns
- 使用 `RegexOptions.Compiled` 缓存正则，类内 `private static readonly`，避免每次调用都编译。

## Dependencies
### Internal
- 被 `../../Poxiao.Lab/Service/RawDataImportSessionService.cs`、`MagneticDataImportSessionService.cs`、`MonthlyQualityReportService.cs` 等大量服务消费。

### External
- `System.Text.RegularExpressions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
