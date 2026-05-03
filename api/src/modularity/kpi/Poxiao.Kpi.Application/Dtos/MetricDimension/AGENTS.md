<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricDimension

## Purpose
公共维度 DTO。维度从某个数据模型中抽出可复用的字段（用作分组/筛选），由 `MetricDimensionService` 维护。

## Key Files
| File | Description |
|------|-------------|
| `MetricDimensionCrInput.cs` | 新建维度（来源模型类型/id、字段、`column` 强类型对象） |
| `MetricDimensionUpInput.cs` | 更新维度 |
| `MetricDimensionListQueryInput.cs` | 列表查询（关键字、分页、排序） |
| `MetricDimensionListOutput.cs` | 列表项 |
| `MetricDimensionInfoOutput.cs` | 详情，`column`/`dataModelId` 反序列化为对象 |
| `MetricDimOptionsOutput.cs` | 选择器/筛选器选项（具体字段值集合） |

## For AI Agents

### Working in this directory
- `Column` 是 `TableFieldOutput`，落库时通过 `MetricDimMapper` 序列化为 JSON 字符串。
- `DataModelId` 是 `DbSchemaOutput`，同样在 Mapper 中 JSON 序列化。

### Common patterns
- DTO 与实体之间的差异只在 JSON 字段上，命名与字段语义保持完全一致。

## Dependencies
### Internal
- `../../Mapper/MetricDimMapper.cs`
- `../../../Poxiao.Kpi.Core/Entities/MetricDimension`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
