<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
定时任务相关 SqlSugar 实体：业务表 `BASE_TIMETASK*` + 调度框架持久化表 `JobDetails` / `JobTriggers`（路由到 `Poxiao-Job` 租户库）。

## Key Files
| File | Description |
|------|-------------|
| `TimeTaskEntity.cs` | `BASE_TIMETASK`：fullName/enCode/executeType (1 接口 / 3 本地)/executeContent (JSON) /executeCycleJson/lastRunTime/nextRunTime/runCount |
| `TimeTaskLogEntity.cs` | `BASE_TIMETASKLOG`：taskId/runTime/runResult/description（继承 `OEntityBase<string>`） |
| `JobDetails.cs` | 调度框架持久化任务信息表（`[Tenant("Poxiao-Job")]`，含 ScriptCode、CreateType=`RequestTypeEnum`） |
| `JobTriggers.cs` | 调度框架触发器表（cron / 间隔 / 状态） |

## For AI Agents

### Working in this directory
- `JobDetails.ScriptCode` 列声明 `ColumnDataType = StaticConfig.CodeFirst_BigString`，CodeFirst 时确保数据库支持长文本。
- 业务任务变更后必须同时回写 `JobDetails`/`JobTriggers`，由 `TimeTaskService` 调用 `ISchedulerFactory` 协同。
- 不要在业务模块中直接 CRUD `JobDetails`，统一走调度服务。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
