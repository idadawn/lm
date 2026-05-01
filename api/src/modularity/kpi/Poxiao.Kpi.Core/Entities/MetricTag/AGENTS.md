<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricTag

## Purpose
指标标签实体及其计数表。标签用于跨分类的扁平筛选，并能记录与指标/思维图等的关联次数。

## Key Files
| File | Description |
|------|-------------|
| `MetricTagsEntity.cs` | 表 `metric_tags` (`CUDEntityBase`)：`name`/`sort`/`description?` |
| `MetricTagsCountEntity.cs` | 表 `metric_tags_count` (`CUDEntityBase`)：`tag_id`/`tag_count_category`/`relation_id` |

## For AI Agents

### Working in this directory
- 实体名复数 `MetricTags*`，与 DTO 单复数混用，请按文件名引用。
- 计数表的 `tag_count_category` 是字符串分类（如 metric/got/cov），`relation_id` 是被关联实体主键。

### Common patterns
- 软删除走基类的 `Delete()`，写入 `delete_time/delete_user_id/is_deleted`。

## Dependencies
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
