<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Repository

## Purpose
工作流数据访问接口。`IFlowTaskRepository` 是工作流模块对外暴露的查询/CRUD 契约，覆盖列表（监控/我发起/待审/已审/抄送/批量/门户）+ 任务/节点/经办/记录/抄送/候选/驳回/委托用户解析 + 任务参数装配。

## Key Files
| File | Description |
|------|-------------|
| `IFlowTaskRepository.cs` | 大型聚合接口，按 region 划分：流程列表、其他模块流程列表、Other、FlowTask、FlowTaskNode、FlowTaskOperator、FlowTaskOperatorRecord、FlowTaskCirculate、FlowTaskCandidates、系统表单、FlowTaskParamter、FlowRejectData |

## For AI Agents

### Working in this directory
- 接口方法过百，新增方法务必放入对应 region 并保持 XML 注释。
- `GetTaskParamterBy*Id` 是构造 `FlowTaskParamter` 的核心入口，被 Manager 频繁复用，慎改签名。
- 列表方法返回 `dynamic` 是历史约定（混合分页 + 用户/部门 join 字段），保留即可。

### Common patterns
- 表达式查询使用 `Expression<Func<T, bool>>`，可附带 `orderBy` 表达式与 `OrderByType`。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Entitys`、`Poxiao.Systems.Entitys.System`、`Poxiao.VisualDev.Entitys`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
