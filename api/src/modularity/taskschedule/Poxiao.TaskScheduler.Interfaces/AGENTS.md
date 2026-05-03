<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.TaskScheduler.Interfaces

## Purpose
定时任务模块对外契约工程。其它模块（system、workflow 等）需要执行调度时通过此接口注入。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.TaskScheduler.Interfaces.csproj` | 工程文件 |
| `ITimeTaskService.cs` | `Task<string> PerformJob(TimeTaskEntity entity)` —— 由实体类型判定执行 HTTP 接口或本地方法 |

## For AI Agents

### Working in this directory
- 仅暴露最小可用入口；其它批量/管理操作请通过 DynamicApi（`api/scheduletask`）调用，不要扩散到接口工程。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
