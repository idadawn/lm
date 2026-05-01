<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDataIE

## Purpose
指标数据导入导出 (Import/Export) DTO。配合 `MetricDataIEService` 在运行时通过 `DynamicProperyBuilder` 创建数据表、生成模板、插入与导出数据。

## Key Files
| File | Description |
|------|-------------|
| `MetricDataIECreateTableInput.cs` | 建表入参：`tableName`、`fieldInfos`、可选首批 `data`（行级字典）；`FieldInfo` 含名称/可空/主键/自增 |
| `MetricDataIECreateTemplateOutput.cs` | 建表模板：可选字段类型与示例 |
| `MetricDataIEInsertTableInput.cs` | 插入数据入参 |
| `MetricDataIEInsertTemplateOutput.cs` | 插入模板 |
| `MetricDataIEExportInput.cs` | 导出入参 |
| `MetricDataIEExportOutput.cs` | 导出结果（含 Excel 文件流/路径） |

## For AI Agents

### Working in this directory
- 字段类型受限于 `MetricDataIEService._fieldInfo`：`整数/长整数/浮点数/字符串/布尔/时间`，不要在 DTO 增设其他类型。
- `Data` 用 `List<Dictionary<string, object>>` 表示一行数据；字段名必须与 `FieldInfos` 对齐。

### Common patterns
- 主键 + 自增组合时落 `int64`，主键非自增则落 `string`，普通字段按 `CodeType` 反射建立。
- 与实体 `MetricDataIETableCollectionEntity` 配合，登记动态创建的表名。

## Dependencies
### Internal
- `Services/MetricDataIE`
- `../../../Poxiao.Kpi.Core/Entities/MetricDataIE`
### External
- Newtonsoft.Json, SqlSugar.DynamicProperyBuilder, NPOI（Excel 处理）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
