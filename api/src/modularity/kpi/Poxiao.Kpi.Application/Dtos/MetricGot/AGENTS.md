<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricGot

## Purpose
指标思维图 (Graph of Thought, GOT) DTO。每张思维图归属一种 `GotType`（价值链 Cov / 仪表板 Dash），可挂指标标签。

## Key Files
| File | Description |
|------|-------------|
| `MetricGotCrInput.cs` | 新建入参（type/sort/name/description/imgName/metricTag） |
| `MetricGotUpInput.cs` | 更新入参 |
| `MetricGotListQueryInput.cs` | 列表查询（关键字、`metricTags` 多选、分页、排序） |
| `MetricGotListOutput.cs` | 列表项（含 `typeStr` 描述） |
| `MetricGotInfoOutput.cs` | 详情 |

## For AI Agents

### Working in this directory
- `MetricTag` 字符串里以逗号分隔多个 tagId，列表查询时与 `MetricTagsEntity` 关联展示名称。
- 所有 DTO 在同一 namespace `Poxiao.Kpi.Application` 下，避免命名冲突时使用具名属性。

### Common patterns
- `GotListOutput.TypeStr` 由服务层 `x.Type?.GetDescription()` 填充，保持枚举描述与前端显示一致。

## Dependencies
### Internal
- `Services/MetricGot`、`Services/MetricTag`、`Core/Enums/GotType`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
