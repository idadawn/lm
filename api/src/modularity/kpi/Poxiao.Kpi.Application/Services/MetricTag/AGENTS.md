<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricTag

## Purpose
指标标签服务。提供标签 CRUD 与分页列表，被指标定义、思维图、价值链等多处引用。

## Key Files
| File | Description |
|------|-------------|
| `IMetricTagsService.cs` | 接口名为复数 `Tags`，与 `MetricTagsEntity` 对齐 |
| `MetricTagsService.cs` | 实现：模糊查询 `Name`，按 `LastModifiedTime`/`CreatedTime` 排序，软删除使用基类 `Delete()` |

## For AI Agents

### Working in this directory
- 实体名复数 `MetricTagsEntity`，DTO/服务命名混用单复数：注意区分 `MetricTagCrInput` 与 `MetricTagsListOutput`。
- 列表返回 `dynamic` (`PageResult<MetricTagsListOutput>.SqlSugarPageResult(data)`)，调用方需处理动态类型。

### Common patterns
- 软删除走 `CallEntityMethod(x => x.Delete())` + 仅更新 `DeleteTime/DeleteUserId/IsDeleted`。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricTag`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
