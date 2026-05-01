<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Config

## Purpose
实验室模块的强类型配置类：一组用于 `appsettings.json` 中 `Lab` 节点（`LabOptions`），另一组是持久化在 `LAB_EXCEL_IMPORT_TEMPLATE.F_TEMPLATE_CONFIG` 字段中的 Excel 模板配置（`ExcelTemplateConfig`）。

## Key Files
| File | Description |
|------|-------------|
| `LabOptions.cs` | `LabOptions(SectionName="Lab")` 包含 `FormulaOptions`：`EnablePrecisionAdjustment`、`DefaultPrecision/MaxPrecision`（默认 6 位）、`UnitPrecisions`（字段名→`UnitPrecisionInfo{UnitId, DecimalPlaces}`）。供公式计算精度兜底链路。 |
| `ExcelTemplateConfig.cs` | Excel 导入模板的完整配置：`FieldMappings`（实体字段 ↔ Excel 列名/列索引/数据类型/单位/小数位）、`DetectionColumns`（检测列模式 `{col}/检测{col}`、最大列数）、`Validation`（必填字段、字段级正则/min/max）、`ExcelHeaders`（保存的 Excel 表头快照，便于编辑回显）。 |

## For AI Agents

### Working in this directory
- `LabOptions` 通过 `Configurations/AppSetting.json` 的 `Lab:Formula:*` 节点绑定；改默认值时务必同步 `Configurations` 目录与文档说明的精度优先级（公式精度 → unitPrecisions → 单位默认精度 → DefaultPrecision）。
- `ExcelTemplateConfig` **是 JSON 序列化模型**，所有属性都标注了 `[JsonPropertyName]`：新增字段需同时考虑前端 `web/src/views/lab/excel-import-template/` 表单兼容。
- `DetectionColumnConfig.Patterns` 中 `{col}` 是占位符（不要改成 `{0}`），`MaxColumn` 默认 100；与 `IntermediateDataColumnAttribute.IsRange` 范围列上限保持一致（流水线常用 22 列）。
- `TemplateColumnMapping.DecimalPlaces` 默认 2 位，仅对数值类型生效；导入时由 `RawDataImportSessionService` 读取并构造 `unitPrecisions` 字典传给计算引擎。

### Common patterns
- 所有持久化为 JSON 的子类（`TemplateColumnMapping/DetectionColumnConfig/TemplateValidationConfig/FieldValidationRule/ExcelHeaderInfo`）都使用 `System.Text.Json` 序列化，`new List<...>()` 默认初始化避免 `null`。

## Dependencies
### Internal
- 被 `Poxiao.Lab/Service/ExcelImportTemplateService.cs`、`RawDataImportSessionService.cs`、`IntermediateDataFormulaBatchCalculator.cs` 消费。

### External
- `System.Text.Json.Serialization`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
