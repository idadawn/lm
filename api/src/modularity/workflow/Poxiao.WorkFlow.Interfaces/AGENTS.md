<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.WorkFlow.Interfaces

## Purpose
工作流引擎的对外接口契约工程，三层接口分离：`Manager`（业务编排）/ `Repository`（数据访问）/ `Service`（动态 API 控制器接口，被 Furion 用于动态路由发现）。其它模块（如 lab/system 模块）仅依赖此项目以避免循环引用。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.WorkFlow.Interfaces.csproj` | 项目工程文件 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Manager/` | 业务编排接口（IFlowTaskManager） (see `Manager/AGENTS.md`) |
| `Repository/` | 数据访问接口（IFlowTaskRepository） (see `Repository/AGENTS.md`) |
| `Service/` | 动态 API 服务接口（IFlowEngineService 等） (see `Service/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 此工程禁止依赖任何具体实现工程（仅依赖 `Poxiao.WorkFlow.Entitys` 与 framework）。
- 接口签名变化必然影响实现，谨慎调整；新增功能优先增加方法而非修改既有签名。

### Common patterns
- 接口与实现一一对应：`IFlowEngineService` ↔ `Poxiao.WorkFlow/Service/FlowEngineService.cs`。

## Dependencies
### Internal
- `Poxiao.WorkFlow.Entitys`、`framework/Poxiao/*`、`modularity/system`、`modularity/visualdev`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
