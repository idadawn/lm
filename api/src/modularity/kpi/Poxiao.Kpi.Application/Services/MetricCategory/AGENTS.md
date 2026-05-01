<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCategory

## Purpose
指标分类树服务。维护 `MetricCategoryEntity`，提供增删改查、分类树（含 `CategoryIdTree`）、下拉选择器，支持迁移子节点。

## Key Files
| File | Description |
|------|-------------|
| `IMetricCategoryService.cs` | CRUD + `GetSelector()` 接口 |
| `MetricCategoryService.cs` | 实现：用 `ITenant` 开启事务，迁移 ParentId 时同步重写所有后代的 `CategoryIdTree` |

## For AI Agents

### Working in this directory
- 同级名称去重：`ParentId.Equals(input.ParentId) && Name.Equals(input.Name)` → `ErrorCode.K10001`。
- 不允许把节点挂到自己的子节点下：`ErrorCode.K10003`。
- 删除前需确认无子节点 (`K10005`)。

### Common patterns
- 根节点 `ParentId == "-1"`；构造树时优先 `.ToTree("-1")`，回退 `.ToTree("0")`。
- 事务模式：`_db.BeginTranAsync` / `CommitTranAsync` / `RollbackTranAsync`。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricCategory`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
