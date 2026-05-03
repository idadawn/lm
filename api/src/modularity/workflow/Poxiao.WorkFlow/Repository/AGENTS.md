<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Repository

## Purpose
工作流数据访问层。`FlowTaskRepository` 实现 `IFlowTaskRepository`，集中处理流程列表查询（监控 / 我发起 / 待我审批 / 我已审批 / 抄送 / 批量审批 / 门户）、`FLOW_TASK` 任务、`FLOW_TASK_NODE` 节点、`FLOW_TASK_OPERATOR(_USER/_RECORD)` 经办、`FLOW_CANDIDATES`、`FLOW_REJECT_DATA`、`FLOW_TASK_CIRCULATE` 等表的 CRUD 与业务投影。

## Key Files
| File | Description |
|------|-------------|
| `FlowTaskRepository.cs` | 单文件大型仓储，按 region 分块：流程列表 / Other / FlowTask / FlowTaskNode / FlowTaskOperator / FlowTaskOperatorRecord / FlowTaskCirculate / FlowTaskCandidates / 系统表单 / FlowTaskParamter / FlowRejectData |

## For AI Agents

### Working in this directory
- 注入 `ISqlSugarRepository<FlowTaskEntity>` + `ISqlSugarClient`，多表写操作使用 `_db.AsTenant()` 显式声明事务。
- 新查询应同时维护 `IFlowTaskRepository` 接口签名与 region 分组，保持现有结构（避免文件混乱）。
- 跨实体联表请使用 `LinqBuilder` / `Expression<Func<…, bool>>` 组合条件，避免拼接字符串。

### Common patterns
- `GetTaskParamterBy{Task|Node|Operator}Id` 是构造 `FlowTaskParamter` 容器的入口，被 Manager 大量复用。
- 列表方法返回 `dynamic`（含分页 + 关联用户/部门字段），避免在 Repository 内做强类型 DTO 投影。
- 删除任务通过 `DeleteFlowTaskAllData` 级联清理 node / operator / record / candidate / reject 数据。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Interfaces`、`Poxiao.WorkFlow.Entitys`、`Poxiao.Systems.Entitys`、`Poxiao.VisualDev.Entitys`、`Poxiao.LinqBuilder`、`Poxiao.Infrastructure.Filter`
### External
- SqlSugar、Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
