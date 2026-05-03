<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.WorkFlow

## Purpose
工作流引擎实现项目。包含动态 API 控制器（`Service/`）、业务编排器（`Manager/`）、数据访问层（`Repository/`）以及内置流程表单实现（`WorkFlowForm/`）。所有具体的发起 / 审批 / 抄送 / 委托 / 模板 / 监控逻辑均在此项目落地。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.WorkFlow.csproj` | 项目文件，引用 Interfaces 与 Entitys 两个工程 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Manager/` | 流程编排核心：FlowTaskManager 及其拆分工具类 (see `Manager/AGENTS.md`) |
| `Repository/` | FlowTaskRepository：流程列表、节点 / 经办 / 记录 / 委托 CRUD (see `Repository/AGENTS.md`) |
| `Service/` | 动态 API 控制器层 (`api/workflow/Engine/*`) (see `Service/AGENTS.md`) |
| `WorkFlowForm/` | 内置请假与销售订单流程表单 (see `WorkFlowForm/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Service 层只做参数校验、用户上下文与异常翻译；业务流转必须委托给 `IFlowTaskManager`。
- 新增审批操作（如「打回上一节点」「批量加签」）时，先在 `Manager/` 中实现并通过 `IFlowTaskManager` 暴露，再在 Service 中加 HTTP 端点。
- DI 生命周期统一使用 `ITransient`（与项目其它模块一致）。

### Common patterns
- 控制器使用 `[ApiDescriptionSettings(Tag="WorkflowEngine"/"WorkflowForm", Order=...)]` 分组排序。
- 异常统一通过 `Oops.Oh(ErrorCode)` / `AppFriendlyException` 抛出。
- Manager 通过 `IServiceScopeFactory` 处理子流程跨作用域调用，避免 SqlSugar 上下文冲突。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Interfaces`、`Poxiao.WorkFlow.Entitys`、`framework/Poxiao/*`、`modularity/system`、`modularity/visualdev`、`modularity/message`
### External
- SqlSugar、Mapster、Furion

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
