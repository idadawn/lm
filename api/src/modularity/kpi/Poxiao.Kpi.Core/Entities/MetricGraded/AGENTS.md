<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricGraded

## Purpose
指标分级实体。给某个 `metric_id` 配置达成/警戒/异常等区间或值，伴随趋势 (`TrendType`) 与状态颜色。

## Key Files
| File | Description |
|------|-------------|
| `MetricGradedEntity.cs` | 表 `metric_graded` (`CUEntityBase`)：`metric_id`/`name`/`type`(`MetricGradeType`)/`rang_type`(`CovRuleValueType?`)/`trend`(`TrendType?`)/`value`(decimal)/`status`/`status_color` |

## For AI Agents

### Working in this directory
- 分级类型 `Value` 时使用 `value`，`Rang` 时建议存最小/最大值（项目当前模型仅一个 `value`，区间需通过 `rang_type` + 趋势组合解释）。
- `status_color` 通常引用 `MetricCovStatusEntity.Color`，便于全局色板一致。

### Common patterns
- DTO `MetricGradedListOutput.Value` 暴露为字符串，规避前端 decimal 精度问题。

## Dependencies
### Internal
- `../../Enums/CovRuleValueType`、`Enums/MetricType`(`MetricGradeType`、`TrendType`)
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
