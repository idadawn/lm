<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Job

## Purpose
Wires `Poxiao.Schedule` (Quartz-style scheduler) into SqlSugar persistence and supports user-defined HTTP / dynamic-script jobs. Hosts the persistence implementation, the dynamic compiler, the built-in `PoxiaoHttpJob`, and shared serialisation helpers.

## Key Files
| File | Description |
|------|-------------|
| `DbJobPersistence.cs` | Implements `IJobPersistence`. `Preload()` fuses `App.EffectiveTypes.ScanToBuilders()` (compile-time `[JobDetail]`) with rows in `JobDetails` / `JobTriggers` so DB-edited cron expressions win. `OnChanged` / `OnTriggerChanged` write back, with multi-tenant routing (column AOP or `AddConnection`+`ChangeDatabase`). Updates `TimeTaskEntity` and emits `TimeTaskLogEntity` rows when triggers fire. |
| `DynamicJobCompiler.cs` | `ISingleton`. `BuildJob(string script)` compiles user C# via `Schedular.CompileCSharpClassCode(script)` and returns the type implementing `IJob` (DotNetCore.Natasha.CSharp underneath). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Converters/` | `DateTimeJsonConverter` for the System.Text.Json options (see `Converters/AGENTS.md`) |
| `Http/` | `PoxiaoHttpJob` + its message DTO (see `Http/AGENTS.md`) |
| `Internal/` | `Penetrates` shared helpers — JSON options, naming conventions, value extraction (see `Internal/AGENTS.md`) |

## For AI Agents

### Working in this directory
- TriggerId convention is `{tenantId}_trigger_schedule_{taskId}`; the regex matches in `OnTriggerChanged` rely on this. Keep that format if you mint new triggers.
- For tenant routing, `GetConnectionScopeWithAttr<JobDetails>()` follows SqlSugar's tenant attribute on `JobDetails`/`JobTriggers`. Don't bypass it; otherwise schedule writes hit the wrong DB.
- Script jobs use `RequestTypeEnum.Http` -> `PoxiaoHttpJob`. Add a new `case` in `Preload` when introducing additional `RequestTypeEnum` variants.

### Common patterns
- `schedulerBuilder.Updated()` is called after every `LoadFrom(dbDetail)` so the persistence layer re-emits writes.
- `App.EffectiveTypes.ScanToBuilders()` only finds types tagged with `[JobDetail]`/`[Trigger]`.

## Dependencies
### Internal
- `Poxiao.Schedule` (`IJobPersistence`, `SchedulerBuilder`, `JobBuilder`, `TriggerBuilder`), `Poxiao.TaskScheduler.Entitys` (JobDetails, JobTriggers, TimeTaskEntity, TimeTaskLogEntity, RequestTypeEnum, TriggerStatus).

### External
- DotNetCore.Natasha.CSharp, Microsoft.CodeAnalysis.CSharp.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
