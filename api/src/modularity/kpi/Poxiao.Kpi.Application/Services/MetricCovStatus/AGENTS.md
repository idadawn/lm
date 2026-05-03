<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCovStatus

## Purpose
价值链节点状态字典服务。提供状态项的 CRUD 以及精简的下拉选项 `GetOptionsAsync`，前端用于状态选择和图例颜色。

## Key Files
| File | Description |
|------|-------------|
| `IMetricCovStatusService.cs` | CRUD + `GetOptionsAsync` 接口 |
| `MetricCovStatusService.cs` | 实现：分页查询带关键字 `Keyword.Contains(it.Name)`，软删除 `DeleteByIdAsync` |

## For AI Agents

### Working in this directory
- `Keyword.Contains(it.Name)` 写法需确保关键字非空（已用 `WhereIF`），否则触发全表扫描。
- 删除走硬删除 `DeleteByIdAsync`，状态字典数据少且需要立即生效。

### Common patterns
- 选项 DTO 仅返回 `Id/Name/Color`，避免暴露审计字段。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricCovStatus`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
