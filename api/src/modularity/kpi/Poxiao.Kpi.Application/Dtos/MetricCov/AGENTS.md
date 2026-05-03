<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MetricCov

## Purpose
指标价值链 (Chain of Value, COV) 及其规则 DTO。承载 `MetricCovService`/`MetricCovRuleService` 与对应 Controller 的入出参。价值链按思维图 (`gotId`) 组织，是指标分级与告警的依据。

## Key Files
| File | Description |
|------|-------------|
| `MetricCovCrInput.cs` | 新建价值链节点入参 |
| `MetricCovUpInput.cs` | 更新节点入参（含 `parentId`/`gotParentId` 双父级） |
| `MetricCovListQueryInput.cs` | 列表查询条件 |
| `MetricCovListOutput.cs` | 列表/树形项，含 `covTreeIds`、`gotTreeId`、`isRoot` |
| `MetricCovInfoOutput.cs` | 节点详情 |
| `MetricCovSelectorOutput.cs` | 下拉/树形选择器（仅 `gotType==Cov`） |
| `MetricCovRuleCrInput.cs` | 价值链规则新建（操作符、值、最小/最大值） |
| `MetricCovRuleUpInput.cs` | 规则更新 |
| `MetricCovRuleListOutput.cs` | 规则列表项 |
| `MetricCovRuleInfoOutput.cs` | 规则详情 |

## For AI Agents

### Working in this directory
- 价值链节点同时维护 `covTreeId`（节点父链）与 `gotTreeId`（思维图父链），两者根 id 都用 `"-1"`。
- 规则字段：`type`/`level`/`operators`/`value`/`minValue`/`maxValue`/`status`，与 `MetricCovRuleEntity` 一一对应。

### Common patterns
- 树形渲染依赖 `ToTree("-1")` 扩展，字段顺序与 `Sort/CreatedTime` 联合排序。
- `gotType` 取值参见 `Core/Enums/GotType` (`Cov`/`Dash`)。

## Dependencies
### Internal
- `../../../Poxiao.Kpi.Core/Entities/MetricCov`、`Enums/GotType`/`CovRuleValueType`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
