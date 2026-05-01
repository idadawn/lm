<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDataIE

## Purpose
动态建表登记实体。`MetricDataIEService` 通过 SqlSugar `DynamicProperyBuilder` 创建外部数据表后，在此登记表名以便审计/排重。

## Key Files
| File | Description |
|------|-------------|
| `MetricDataIETableCollectionEntity.cs` | 表 `metric_table_collection`（自定义最简实体，未继承 `CUDEntityBase`）：`id` 主键 + `table_name` + `created_time`，含本地 `Create()` 方法填充时间与雪花 ID |

## For AI Agents

### Working in this directory
- 该实体是少数不继承 `CUDEntityBase`/`CUEntityBase` 的实体；保持其精简，不要随意添加审计字段。
- `Create()` 方法在 `Insertable.CallEntityMethod` 中调用，自动设置时间和 ID。

### Common patterns
- 表名重复检查由调用方先 `FirstAsync(it => it.TableName == ...)`。

## Dependencies
### External
- SqlSugar
- `Poxiao.Infrastructure.SnowflakeIdHelper`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
