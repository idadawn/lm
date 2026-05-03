<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Db

## Purpose
数据源/Schema/字段查询 DTO。给基础与派生指标的“数据模型选择器”提供数据源列表、表/视图、字段、排序候选与筛选数据查询能力。由 `DbService` 配合 `IDbLinkService`/`IDataBaseManager`/`IInfluxDBManager` 输出。

## Key Files
| File | Description |
|------|-------------|
| `DataModel4DbOutput.cs` | 数据源（数据库连接）选项 |
| `SchemaInfoOutput.cs` | 表/视图详情：`schemaStorageType`、`dbType`、`schemaName`、`host` |
| `ModelDataListOutput.cs` | 模型字段值列表（用于筛选下拉） |
| `ModelDataQueryInput.cs` | 字段值查询条件 |
| `OrderByFieldOutput.cs` | 可排序字段输出 |

## For AI Agents

### Working in this directory
- `SchemaStorageType` 来自 `Poxiao.Kpi.Core.Enums`：`Table`/`View`/`RealTime`。`RealTime` 走 `IInfluxDBManager`。
- 字段命名严格匹配前端：`schemaStorageType`、`schemaName`、`sortCode`，不要驼峰大小写改动。

### Common patterns
- 多数 DTO 由 Mapster 从 `DbLinkListOutput`/`DbTableInfo` 转换而来（参见 `Mapper/MetricCategoryMapper.cs`）。

## Dependencies
### Internal
- `Services/MetricInfo/DbService.cs`
- `Poxiao.Systems.Interfaces`（`IDbLinkService`）
- `Poxiao.Common`（`DbLinkEntity`、`DbTableInfo`）
### External
- Newtonsoft.Json, SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
