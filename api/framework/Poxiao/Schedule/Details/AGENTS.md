<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Details

## Purpose
作业元数据载体 `JobDetail` 的定义与配置选项。封装 JobId/GroupName/JobType/AssemblyName/Concurrent/Properties/Blocked/RuntimeJobType/DynamicExecuteAsync 等字段，是触发器、持久化器与执行器之间共用的"作业身份"。

## Key Files
| File | Description |
|------|-------------|
| `JobDetail.cs` | partial 主体，声明所有 `[JsonInclude]` 公开字段与 internal 运行时字段（`Blocked`、`RuntimeJobType`、`RuntimeProperties`、`DynamicExecuteAsync`）。 |
| `JobDetail.Methods.cs` | partial 方法实现：JSON / SQL / Monitor 序列化、Property 读写、Equals 等运行时辅助方法。 |
| `JobDetailOptions.cs` | 全局静态选项：`LogEnabled`（详细日志开关）与 `ConvertToSQL` 重写委托，配置后通过 `InternalLogEnabled` / `ConvertToSQLConfigure` 流向 SQL 生成。 |

## For AI Agents

### Working in this directory
- 不要直接 `new JobDetail()`；必须通过 `JobBuilder` / `Triggers.From` / `IScheduler.GetJobDetail()` 间接获得，所有 setter 都是 `internal`。
- 新增字段时同时更新 `JobDetail.Methods.cs` 中 `ConvertToJSON`/`ConvertToSQL`/`ConvertToMonitor` 与 `JobBuilder` 的属性映射。
- `Blocked`、`RuntimeJobType`、`RuntimeProperties` 是运行时辅助字段，不要序列化或暴露到 API 返回。

### Common patterns
- 持久化时通过 `JobDetailOptions.ConvertToSQLConfigure` 自定义 SQL 生成，否则使用内置规则按 `NamingConventions` 输出。
- `Concurrent=false` 时调度器读取 `Blocked` 来阻塞下一轮触发（与 `ScheduleHostedService.CheckIsBlocked` 协作）。

## Dependencies
### Internal
- 关联 `../Constants/PersistenceBehavior`、`../Constants/NamingConventions`、`../Triggers/Trigger`。
### External
- `System.Text.Json.Serialization` 用于 `[JsonInclude]`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
