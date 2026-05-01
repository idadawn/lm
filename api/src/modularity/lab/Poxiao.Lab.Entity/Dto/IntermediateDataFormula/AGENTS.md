<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# IntermediateDataFormula

## Purpose
中间数据公式维护 DTO。承载 CALC/JUDGE/NO 公式的 CRUD 数据，并向前端公式编辑器返回可用列、变量来源等元数据。

## Key Files
| File | Description |
|------|-------------|
| `IntermediateDataFormulaDto.cs` | 公式记录：`Id/SourceType(SYSTEM/CUSTOM)/TableName/ColumnName/DisplayName/FormulaName/Formula/FormulaLanguage(EXCEL/MATH)/FormulaType(CALC/JUDGE/NO)/DefaultValue/Precision/UnitId/Enabled/Sort` 等；服务端按 `(FormulaType, ColumnName)` 分组取启用记录。 |
| `IntermediateDataAvailableColumnsResult.cs` | 公式编辑下拉项：列出 `IntermediateDataEntity` 中 `[IntermediateDataColumn(ShowInFormulaMaintenance=true)]` 的列。 |

## For AI Agents

### Working in this directory
- DTO 字段名与数据库 `LAB_INTERMEDIATE_DATA_FORMULA` 列严格对应，新增字段需同步实体 `IntermediateDataFormulaEntity` 与 `IntermediateDataFormulaService`。
- `Formula` 表达式由 `FormulaParser` 解析；JUDGE 类型的 `Formula` 是 JSON 数组（rule 列表，每条含 `resultValue/rootGroup` 或 `groups`）——前端编辑器按 `FormulaType` 切换 UI。
- `DefaultValue` 字段语义因类型而异：CALC 公式表示空值兜底，JUDGE 公式表示无规则匹配时的默认等级名（由 `generate-judgment` 自动写入）。
- 不要把单位换算逻辑放在 DTO；交给 `Poxiao.Lab/Service/IntermediateDataFormulaCalcHelper.cs` 与 `UnitConversionService`。

### Common patterns
- 字段全部使用 `[JsonPropertyName]` camelCase 暴露给前端公式编辑器。

## Dependencies
### Internal
- `../../Entity/IntermediateDataFormulaEntity.cs`、`Enums/IntermediateDataFormulaType.cs`。
- `../../Poxiao.Lab.Interfaces/IIntermediateDataFormulaService.cs`、`IFormulaParser.cs`。

### External
- System.Text.Json。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
