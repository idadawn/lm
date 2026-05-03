<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricInfo

## Purpose
指标定义实体。基础/派生/复合三类指标共用一张表，靠 `type` 字段区分。是 KPI 模块的中心实体，被取数、分析、仪表板等多处引用。

## Key Files
| File | Description |
|------|-------------|
| `MetricInfoEntity.cs` | 表 `metric_info` (`CUDEntityBase`)：`sort?`、`type`(`MetricType`)、`name`/`code`、`date_model_type`(`DataModelType`)、`data_model_id`/`column`/`agg_type?`/`format`/`expression`/`dimensions?`/`time_dimensions?`/`display_mode`(`MetricDisplayMode`)/`filters?`、`metric_category?`/`metric_tag?`/`parent_id`、`derive_type?`、`ca_granularity?`/`date_granularity?`(`GranularityType?`)、`granularity_str?`、`ranking_type?`/`sort_type?`、`formula_data?`、`format_value?`、`is_enabled`、`description?`、`frequency`(`StorageFqType`)、`data_type`(`MetricDataType`，默认 `Static`) |

## For AI Agents

### Working in this directory
- 该实体字段较多，是一个"超表"：基础指标用 `data_model_id/column/agg_type/format`；派生指标增加 `derive_type/granularity*/ranking_type/sort_type`；复合指标使用 `expression/formula_data/parent_id`(逗号分隔多 id)。
- 所有枚举列均使用 `SqlParameterDbType = typeof(EnumToStringConvert)` 落字符串。
- JSON 字段 (`data_model_id/column/format/dimensions/filters/time_dimensions/granularity_str`) 都是 `string`/`string?`，DTO 转换由 `MetricInfoMapper` 处理。

### Common patterns
- `data_type=RealTime` 触发实时取数（InfluxDB），`Static` 走关系库。
- `parent_id` 默认 `"-1"` 表示无父级；复合指标时为逗号分隔的多个父 id。

## Dependencies
### Internal
- `../../Enums/MetricType`、`Enums/DataModelType`、`Enums/GranularityType`、`Enums/StorageFqType`、`Poxiao.Infrastructure.Enums`(`DBAggType`/`DBSortByType`)
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
