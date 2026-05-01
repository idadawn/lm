<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCovStatus

## Purpose
价值链状态字典实体。仅 `name`/`color`，被价值链节点、指标分级等模块共享，作为全局状态色板。

## Key Files
| File | Description |
|------|-------------|
| `MetricCovStatusEntity.cs` | 表 `metric_cov_status` (`CUEntityBase`)：`name`/`color` |

## For AI Agents

### Working in this directory
- 表非常窄（只有 2 列业务字段），不要在此堆积扩展配置；新增需求请评估是否新建实体。
- `color` 是前端能直接用的颜色字符串（CSS color / token），实体不做格式校验。

### Common patterns
- 由于状态项数量小，服务层使用硬删除 (`DeleteByIdAsync`)。

## Dependencies
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
