<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
实验室模块自定义 C# 特性，挂在实体属性上以驱动两个关键流程：Excel 导入列映射、中间数据列在公式维护界面与计算引擎中的元数据。

## Key Files
| File | Description |
|------|-------------|
| `ExcelImportColumnAttribute.cs` | 标识 `RawDataEntity` 字段对应的 Excel 列名 (`Name`)、排序 (`Sort`)、是否参与导入 (`IsImportField`)。Excel 解析器据此构建表头映射。 |
| `IntermediateDataColumnAttribute.cs` | 标识 `IntermediateDataEntity` 字段在公式维护中的元数据：`DisplayName/IsCalculable/Sort/DataType/ShowInFormulaMaintenance/Description/Unit/DecimalDigits`，并支持范围列配置 (`IsRange/RangeStart/RangeEnd/RangePrefix`，例如 `Detection1..22`)。 |

## For AI Agents

### Working in this directory
- 新增中间数据列时**必须**同时配置 `[IntermediateDataColumn]`，否则该列不会出现在公式维护下拉、不会被 `IIntermediateDataFormulaService.GetAvailableColumnsAsync` 返回。
- 设置 `IsRange=true` 时务必同步给出 `RangePrefix`（如 `Detection`、`Thickness`、`LaminationDist`），否则公式 `[Start] TO [End]` 范围展开会失败。
- `DecimalDigits` 是**可空 int**，C# 属性参数不允许可空类型作构造参数 → 只能用对象初始化器赋值（注释中已说明）。
- `Unit` 字段被 `IntermediateDataFormulaCalcHelper`/`UnitDefinitionEntity.ScaleToBase` 链路用于单位换算与精度优先级；与 `Config/LabOptions.UnitPrecisions` 形成层级。

### Common patterns
- 同一实体属性可同时挂 `[ExcelImportColumn(...)]` 和 `[SugarColumn(ColumnName=...)]`（见 `Entity/RawDataEntity.cs`）。
- 默认 `DataType="decimal"`、`IsCalculable=true`、`ShowInFormulaMaintenance=true`：仅在文本/状态字段需显式覆盖。

## Dependencies
### Internal
- 被 `Poxiao.Lab.Entity/Entity/IntermediateDataEntity.cs`、`RawDataEntity.cs` 等实体使用。
- 被 `Poxiao.Lab/Service/IntermediateDataFormulaService.cs`、`IntermediateDataFormulaBatchCalculator.cs` 通过反射读取。

### External
- 仅 `System` 命名空间。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
