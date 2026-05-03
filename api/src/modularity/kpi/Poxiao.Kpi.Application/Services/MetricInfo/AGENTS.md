<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricInfo

## Purpose
指标定义核心服务集合。涵盖基础指标 (`MetricInfoService`)、派生指标 (`MetricInfo4DeriveService`)、复合指标 (`MetricInfo4CompositeService`)，以及为前端选择数据源/Schema 的 `DbService`。

## Key Files
| File | Description |
|------|-------------|
| `IMetricInfoService.cs` | 基础指标 CRUD + 启用切换 + 聚合方式列表 + 维度查询 + 对话端 `GetAll4ChatAsync`/`GetByNameAsync` |
| `MetricInfoService.cs` | 实现：注入 `IUserManager`/`IDbService`/`IInfluxDBManager`；`DealDataAsync` 反序列化 JSON 字段；列表带分类/标签/类型多条件过滤 |
| `IMetricInfo4DeriveService.cs` | 派生指标 CRUD + 校验 |
| `MetricInfo4DeriveService.cs` | 实现：调用基础服务校验 name/code，构造 `entity.GranularityStr` 等 JSON 字段 |
| `IMetricInfo4CompositeService.cs` | 复合指标 CRUD + `FormulaCheckAsync` |
| `MetricInfo4CompositeService.cs` | 实现：用 NCalc 解析公式 (`Expression`)，错误抛 `K10018/K10020` |
| `IDbService.cs` / `DbService.cs` | 数据源、Schema、字段、聚合、筛选数据查询；支持 SqlServer/MySQL/Oracle/InfluxDB |

## For AI Agents

### Working in this directory
- 三类指标共享同一张 `MetricInfoEntity` 表，靠 `Type` (`Basic/Derive/Composite`) 区分；CRUD 时需同步设置默认 `DisplayMode`。
- 重复名称/编码校验：`CheckNameAsync` → `K10010`、`CheckCodeAsync` → `K10011`。
- 公式解析：`input.FormulaData.Replace("${", "").Replace("}", "")` 后交给 `NCalc.Expression.Evaluate()`。

### Common patterns
- 取数前 `DealDataAsync` 反序列化 `dimensions/filters/format/timeDimensions/dataModelId/column`。
- `DbService` 依赖 `IDataBaseManager`、`IDbLinkService`、`IUserManager`、`IInfluxDBManager`、`IJsonClient`。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricInfo`、`Enums/MetricType`/`DataModelType`/`StorageFqType`
- `Poxiao.Systems.Interfaces.System` (`IDbLinkService`)、`Poxiao.Common`(`IDataBaseManager`/`IInfluxDBManager`)
### External
- SqlSugar, NCalcSync, Microsoft.Extensions.Logging

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
