<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCog

## Purpose
指标图链实体。把一个指标关联到一组图形 (`chain_of_graph_ids`)，并支持父子结构。

## Key Files
| File | Description |
|------|-------------|
| `MetricCogEntity.cs` | 表 `metric_cog` (`CUDEntityBase`)：`metric_id`/`parent_id`(`long?`)/`chain_of_graph_ids` |

## For AI Agents

### Working in this directory
- `parent_id` 类型 `long?`，与其他模块的 `string` 父级字段不同，迁移/对接时注意转换。
- `chain_of_graph_ids` 是图形链字符串（具体分隔符与上层服务一致）。

### Common patterns
- 与 DTO 通过 Mapster `Adapt` 直接映射，无 JSON 字段需特殊处理。

## Dependencies
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
