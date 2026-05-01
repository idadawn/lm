<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricTag

## Purpose
指标标签 DTO。标签用于跨分类的扁平筛选，与思维图、指标定义关联。

## Key Files
| File | Description |
|------|-------------|
| `MetricTagCrInput.cs` | 新建标签（name/sort/description） |
| `MetricTagUpInput.cs` | 更新标签 |
| `MetricTagListQueryInput.cs` | 列表查询条件 |
| `MetricTagListOutput.cs` | 列表项（含审计字段，沿用 `MetricTagsListOutput` 命名） |
| `MetricTagInfoOutput.cs` | 标签详情 |

## For AI Agents

### Working in this directory
- 实体名为 `MetricTagsEntity`（复数）；DTO 部分类用单数 `MetricTag*`，注意拼写差异。
- `Sort` 为 `long`，前端按升序展示。

### Common patterns
- 服务层 `MetricTagsService.GetListAsync` 返回 `dynamic`/`PageResult<MetricTagsListOutput>`，分页字段沿用项目通用 `PageResult`。

## Dependencies
### Internal
- `Services/MetricTag`、`Core/Entities/MetricTag`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
