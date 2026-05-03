<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCovStatus

## Purpose
价值链节点状态字典 DTO。维护一组带颜色的状态标签（如“正常/预警/异常”），供价值链节点和分级规则引用。

## Key Files
| File | Description |
|------|-------------|
| `MetricCovStatusCrInput.cs` | 新建状态（名称、颜色） |
| `MetricCovStatusUpInput.cs` | 更新状态 |
| `MetricCovStatusListQueryInput.cs` | 列表查询（关键字、分页、排序） |
| `MetricCovStatusListOutput.cs` | 列表项 |
| `MetricCovStatusInfoOutput.cs` | 详情 |
| `MetricCovStatusOptionOutput.cs` | 选择器选项（id/name/color） |

## For AI Agents

### Working in this directory
- 颜色字段为前端可识别字符串（CSS 颜色或 token），具体校验放在前端/服务层。
- 选项 DTO 字段精简，方便前端下拉直接使用。

### Common patterns
- 服务侧返回 `PagedResultDto<MetricCovStatusListOutput>`。
- 与 `MetricGradedEntity.StatusColor` 形成约束链。

## Dependencies
### Internal
- `Services/MetricCovStatus`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
