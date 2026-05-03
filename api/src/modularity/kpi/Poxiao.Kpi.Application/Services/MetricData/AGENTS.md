<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricData

## Purpose
指标取数服务。根据 `MetricInfoEntity.Type` 路由到基础/派生/复合处理，并通过 `IDbService` 聚合数据库数据，再用 `DataModelFormat` 做格式化。

## Key Files
| File | Description |
|------|-------------|
| `IMetricDataService.cs` | `GetDataAsync(metricId, displayOption?)`、`GetChartDataAsync(input)`、`GetMoreChartDataAsync(input)` |
| `MetricDataService.cs` | 实现：注入 `IDbService`/`IMetricInfoService`/`IMetricGradedService`/`ILogger`；处理基础/复合/实时三类取数 |

## For AI Agents

### Working in this directory
- 基础指标分两路：`MetricDataType.RealTime` 走 `GetRealDataAsync`（InfluxDB），否则 `GetBasicDataAsync` 走关系库。
- 时间维度优先级：`info.TimeDimensions != null` → 用时间维度；否则取 `info.Dimensions[0]`；都没有则返回聚合单值。
- 格式化失败仅记日志，不抛异常（保留原始数据）。

### Common patterns
- `ModelDataAggQueryInput` 字段：`TaskId`(雪花)、`LinkId`、`SchemaName`、`ColumnField`、`AggType`、`Filters`、`Dimensions`、`Granularity`、`DisplayOption`。
- 复合指标在 `GetCompositeDataAsync` 中递归求各父指标值再用 NCalc 求公式。

## Dependencies
### Internal
- `../MetricInfo`、`../MetricIGrade`
### External
- SqlSugar, Microsoft.Extensions.Logging, NCalcSync

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
