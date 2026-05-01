<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCategory

## Purpose
指标分类相关实体。`MetricCategoryEntity` 是树形分类目录，`MetricCategoryCountEntity` 记录分类与指标的多对多关系（用于计数/快速查询）。

## Key Files
| File | Description |
|------|-------------|
| `MetricCategoryEntity.cs` | 表 `metric_category` (`CUDEntityBase`)：`name`/`sort`/`own_id`/`parent_id`/`category_id_tree`/`description` |
| `MetricCategoryCountEntity.cs` | 表 `metric_category_count` (`CUDEntityBase`)：`category_id`/`metric_id` |

## For AI Agents

### Working in this directory
- 根节点 `parent_id = "-1"`；`category_id_tree` 为逗号分隔祖先链（root→...→self）。
- 分类计数表用于跨表的反向查询，新增/删除指标时需同步维护。

### Common patterns
- `Sort` 用于前端排序（`OrderBy(x => x.Sort)`）。

## Dependencies
### Internal
- `../../../common/Poxiao.Common`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
