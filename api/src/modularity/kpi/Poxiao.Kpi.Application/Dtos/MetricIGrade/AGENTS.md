<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricIGrade

## Purpose
指标分级 (Metric Graded / Indicator Grade) DTO。把指标按区间或值划分为不同等级（达成/警戒/异常），驱动颜色与状态展示。

## Key Files
| File | Description |
|------|-------------|
| `MetricInfoGradeCrInput.cs` | 新建分级入参（metricId、name、值/区间、趋势、状态） |
| `MetricInfoGradeExtInput.cs` | 扩展批量/复合入参 |
| `MetricGradedUpInput.cs` | 更新分级 |
| `MetricGradedListOutput.cs` | 列表项（值、区间类型、状态、状态颜色） |
| `MetricGradedInfoOutput.cs` | 详情 |

## For AI Agents

### Working in this directory
- `MetricGradeType` 枚举：`Value`（值）/`Rang`（区间）；`CovRuleValueType` 区分数值/百分比；`TrendType` 上升/下降。
- 颜色字段沿用价值链状态体系（`MetricCovStatusEntity`），便于全局色板。

### Common patterns
- 服务层按 `metricId` 分组列出全部分级，`OrderBy(CreatedTime).OrderBy(Status)`。
- DTO 中的 `Value` 暴露为字符串以容纳百分比格式。

## Dependencies
### Internal
- `Services/MetricIGrade`、`Core/Entities/MetricGraded`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
