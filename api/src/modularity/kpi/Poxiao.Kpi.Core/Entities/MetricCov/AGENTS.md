<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCov

## Purpose
价值链节点与规则实体。价值链节点 (`MetricCovEntity`) 同时维护 `parent_id`/`got_parent_id` 双父链，对应不同视图；价值链规则 (`MetricCovRuleEntity`) 给节点附加阈值判定。

## Key Files
| File | Description |
|------|-------------|
| `MetricCovEntity.cs` | 表 `metric_cov` (`CUEntityBase`)：`name`、`got_type`(`GotType`)、`got_id`、`metric_id?`、`parent_id?`/`cov_tree_id`/`is_root`/`got_parent_id?`/`got_tree_id` |
| `MetricCovRuleEntity.cs` | 表 `metric_cov_rule` (`CUEntityBase`)：`cov_id`、`level`、`type`、`operators`、`value?`/`min_value?`/`max_value?`、`status?` |

## For AI Agents

### Working in this directory
- 双父链：`cov_tree_id` 描述节点本身的父链；`got_tree_id` 描述节点在思维图上的父链；根节点二者相等。
- `is_root` 标识根节点；`got_id` 关联 `MetricGotEntity`。
- 规则字段 `value/min_value/max_value` 都是 `decimal?`，按操作符 (`GreaterThan/Between/LessThan`) 取用。

### Common patterns
- 服务层会重写后代节点的 `cov_tree_id` 和 `got_tree_id`，请保持字段一致性。

## Dependencies
### Internal
- `../../Enums/GotType`、`Enums/CovRuleValueType`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
