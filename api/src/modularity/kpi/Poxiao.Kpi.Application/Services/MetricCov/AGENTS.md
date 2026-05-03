<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCov

## Purpose
指标价值链 (COV) 与其规则的服务。`MetricCovService` 维护节点树与 `gotTreeId` 双父链；`MetricCovRuleService` 维护节点上的告警/状态判定规则。

## Key Files
| File | Description |
|------|-------------|
| `IMetricCovService.cs` | 节点 CRUD、按 `gotId` 取列表、按 `tag` 取 KPI 列表、`GetSelector` |
| `MetricCovService.cs` | 实现：事务化 `_db`、维护 `covTreeId` 与 `gotTreeId`、调用 `IMetricGotService` 获取标签关联 |
| `IMetricCovRuleService.cs` | 规则 CRUD（按 covId） |
| `MetricCovRuleService.cs` | 规则实现：直接 `Adapt` 入库 |

## For AI Agents

### Working in this directory
- 同级 (`parentId+gotId+name`) 节点重名 → `ErrorCode.K10012`；不存在 → `K10013`；自挂子节点 → `K10014`；事务失败 → `K10015`；存在子节点不可删 → `K10022`。
- 根节点创建时 `IsRoot=true`，`gotTreeId == covTreeId`；非根节点单独维护 `gotTreeId`。
- 修改 `ParentId`/`GotParentId` 后，同步重写所有后代节点对应字段。

### Common patterns
- `GetKpiListAsync(tag)` 经 `IMetricGotService.GetGotIdByTag(tag)` 取得 GotId 集合，再过滤价值链。
- 树形渲染：`ToTree("-1")`/`"0"` 双兼容。

## Dependencies
### Internal
- `../MetricGot`、`../../../Poxiao.Kpi.Core/Entities/MetricCov`、`Enums/GotType`
### External
- SqlSugar, Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
