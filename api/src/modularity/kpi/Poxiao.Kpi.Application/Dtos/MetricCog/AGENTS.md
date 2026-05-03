<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCog

## Purpose
指标图链 (Chain of Graph) DTO。对应 `MetricCogService`/`MetricCogController`，记录指标在多张图之间的连接关系（`chainOfGraphIds`）。

## Key Files
| File | Description |
|------|-------------|
| `MetricCogCrInput.cs` | 新建图链入参（指标 id、父级、图形链 id） |
| `MetricCogUpInput.cs` | 更新入参 |
| `MetricCogListQueryInput.cs` | 列表查询（按 metricId 模糊匹配，分页/排序） |
| `MetricCogListOutput.cs` | 列表项（含审计字段） |
| `MetricCogInfoOutput.cs` | 详情 |

## For AI Agents

### Working in this directory
- `chainOfGraphIds` 为字符串形式的图形链拼接（具体分隔规则同实体存储）。
- `parentId` 在实体里是 `long?`，DTO 视情况暴露字符串形式以便前端处理。

### Common patterns
- 命名空间统一 `Poxiao.Kpi.Application`，无独立 namespace。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricCog`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
