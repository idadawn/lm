<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDataIE

## Purpose
指标数据导入导出与动态建表服务。基于 SqlSugar `DynamicProperyBuilder` 在运行时建表，支持模板生成、批量插入与导出。建表记录登记到 `MetricDataIETableCollectionEntity`。

## Key Files
| File | Description |
|------|-------------|
| `IMetricDataIEService.cs` | 接口：`CreateDBTable`/`InsertData`/`Export*`/`Get*Template` |
| `MetricDataIEService.cs` | 实现：内置 `_fieldInfo` 限定字段类型映射，`FieldRule` 校验，`CodeFirst.InitTables(table)` 真正建表 |

## For AI Agents

### Working in this directory
- 字段类型受限于内置 `_fieldInfo` 集合：整数/长整数/浮点数/字符串/布尔/时间，新增类型必须先扩展该列表。
- 表名重复返回 `(false, "'<name>'表已存在")`，不会抛异常；调用方应据此决定 `Oops.Bah(msg)`。
- 主键 + 自增组合限定为 `int64`；纯主键非自增为 `string`。

### Common patterns
- 动态类型构造：`_db.DynamicBuilder().CreateClass(name, new SugarTable())` → `CreateProperty(name, Type.GetType(codeType), sugarColumn)` → `WithCache().BuilderType()`。
- 建表后立即写入 `MetricDataIETableCollectionEntity`，便于后续审计。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricDataIE`
### External
- SqlSugar (DynamicProperyBuilder), NPOI, Reflection

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
