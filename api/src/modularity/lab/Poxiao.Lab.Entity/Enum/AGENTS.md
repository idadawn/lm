<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enum

## Purpose
实验室模块业务枚举（不直接影响公式计算流水线状态机），含质量状态与 Excel 模板编码。所有枚举使用 `[Description]` 暴露中文显示名。

## Key Files
| File | Description |
|------|-------------|
| `QualityStatusEnum.cs` | `Qualified(合格)/Unqualified(不合格)/Other(其他)`，挂在 `IntermediateDataJudgmentLevelEntity.QualityStatus` 与 `JudgmentLevelDto.QualityStatus` 上，驱动月度质量报表的合格率统计与列分类。 |
| `ExcelImportTemplateCode.cs` | `RawDataImport(检测数据导入模板)/MagneticDataImport(磁性数据导入模板)`，标识两条不同 Excel 解析路径。 |

## For AI Agents

### Working in this directory
- `QualityStatusEnum.Other` 不应在质量报表合格率分子分母中被计入；遇到归类不明的等级也用 `Other` 兜底。
- 新增 Excel 模板代码（如供应商指标、外观巡检表）时，需要同步在 `Poxiao.Lab/Service/ExcelImportTemplateService.cs` 中注册解析器并扩展前端模板下拉。
- 该目录与 `Enums/`（流水线状态枚举）刻意分开维护：本目录是面向最终用户的业务语义，`Enums/` 是面向计算引擎的内部状态。

### Common patterns
- 所有枚举值都有 `[Description]` 中文标签，前端通过基础设施 `EnumUseNameConverter` 序列化时输出名称。

## Dependencies
### Internal
- 被 `../Entity/IntermediateDataJudgmentLevelEntity.cs`、`ExcelImportTemplateEntity.cs`、`../Dto/IntermediateDataJudgmentLevel/`、`MonthlyQualityReport/` 引用。

### External
- `System.ComponentModel.Description`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
