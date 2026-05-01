<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDimension

## Purpose
公共维度实体。从某个数据模型中抽取的字段，可被多个指标作为维度共用。

## Key Files
| File | Description |
|------|-------------|
| `MetricDimensionEntity.cs` | 表 `metric_dimension` (`CUEntityBase`)：`date_model_type`/`data_model_id`(JSON 字符串)/`name`/`data_type`/`column`(JSON 字符串) |

## For AI Agents

### Working in this directory
- `data_model_id`/`column` 落库为 JSON 字符串，DTO 端通过 `MetricDimMapper` 反序列化为 `DbSchemaOutput`/`TableFieldOutput`。
- `date_model_type` 为字符串列，对应 `Core/Enums/DataModelType` 的枚举名字。

### Common patterns
- 列表查询使用 `Keyword.Contains(it.Name)` 模糊匹配。

## Dependencies
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
