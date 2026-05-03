<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCog

## Purpose
指标图链服务。维护 `MetricCogEntity`：每个指标可关联多张图形 (`chainOfGraphIds`)，支持父子关系。

## Key Files
| File | Description |
|------|-------------|
| `IMetricCogService.cs` | 标准 CRUD + 列表查询接口 |
| `MetricCogService.cs` | 实现：分页 `ToPagedListAsync` + `PageResult.SqlSugarPageResult`；软删除回写 `DeleteTime/DeleteUserId/IsDeleted` |

## For AI Agents

### Working in this directory
- `MetricId` 字段使用 `Contains(input.MetricId)` 模糊匹配，注意性能。
- 雪花 ID：`SnowflakeIdHelper.NextId()`。

### Common patterns
- 排序：未指定 `Sidx` 时按 `Id`，否则 `Sidx + " " + Sort`（注意 SQL 注入风险，需确保 Sidx 来自白名单）。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricCog`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
