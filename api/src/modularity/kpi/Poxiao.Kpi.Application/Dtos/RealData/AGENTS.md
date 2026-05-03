<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# RealData

## Purpose
实时数据查询 DTO。当指标 `MetricDataType=RealTime` 时，`MetricDataService` 走 InfluxDB/实时通道取数；这里定义查询参数。

## Key Files
| File | Description |
|------|-------------|
| `RealDataQryInput.cs` | 实时查询：`name`、`key`、`limit`、`sortBy` (`DBSortByType.ASC` 默认) |
| `RealDataAggQueryInput.cs` | 聚合实时查询：增加 `min`（默认 60 分钟） |

## For AI Agents

### Working in this directory
- 字段命名混合大小写（`key` 小写）以兼容已有前端调用，请勿重命名。
- 排序字段由 `Poxiao.Infrastructure.Enums.DBSortByType` 提供。

### Common patterns
- 上层 `MetricDataService.GetRealDataAsync` 调用 `IInfluxDBManager` 完成实际查询。

## Dependencies
### Internal
- `Services/MetricData`、`Poxiao.Infrastructure`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
