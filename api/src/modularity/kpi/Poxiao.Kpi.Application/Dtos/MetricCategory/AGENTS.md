<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCategory

## Purpose
指标分类（树形目录）DTO。`MetricCategoryService` 与 `MetricCategoryController` 用其完成增删改查、分类树查询与下拉选择器。

## Key Files
| File | Description |
|------|-------------|
| `MetricCategoryCrInput.cs` | 新建分类入参（名称、父级、排序、描述） |
| `MetricCategoryUpInput.cs` | 更新分类入参 |
| `MetricCategoryListQueryInput.cs` | 列表查询条件（关键字、是否含已删除、分页） |
| `MetricCategoryListOutput.cs` | 列表/树形项，继承 `TreeModel`，含 `categoryIdTree`、`categoryIds` |
| `MetricCategoryInfoOutput.cs` | 单条详情 |
| `MetricCategorySelectorAllOutput.cs` | 下拉/选择器视图 |

## For AI Agents

### Working in this directory
- 父级根节点使用 `"-1"`，列表会按 `Sort` 升序后调用 `.ToTree("-1")`/`"0"` 扩展。
- 字段命名向前端兼容：`fullName`/`organizeIdTree`/`organizeIds` 通过 `[JsonProperty]` 映射，不要轻易改动。

### Common patterns
- `CategoryIdTree` 拼接祖先 Id（逗号分隔），DTO 端额外暴露 `CategoryIds` 列表。
- 与实体 `MetricCategoryEntity` 通过 `Adapt<>()` 互转。

## Dependencies
### Internal
- `Services/MetricCategory`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
