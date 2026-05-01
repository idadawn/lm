<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Builders

## Purpose
Fluent, programmatic alternative to the attribute-driven scheduler API. `JobBuilder` produces a `JobDetail`, `TriggerBuilder` produces a `Trigger`, `SchedulerBuilder` binds a `JobBuilder` to a list of `TriggerBuilder`s, and `ScheduleOptionsBuilder` is the root `IServiceCollection.AddSchedule(opts => ...)` configuration object that assembles everything (executor, monitor, persistence, cluster server, factory, UTC flag, unobserved-task handler).

## Key Files
| File | Description |
|------|-------------|
| `JobBuilder.cs` | `sealed` builder extending `JobDetail`. Static `Create(jobId)` / `Create<TJob>()` / `Create(Type)` / `Create(asmName, fullName)` factories. |
| `TriggerBuilder.cs` | `partial` `sealed` builder extending `Trigger`. Static helpers `Period(intervalMs)` and `Cron(schedule, format)` plus the macro overloads. |
| `TriggerBuilder.Setters.cs` | Fluent setters partial — `SetTriggerId`, `SetStartTime`, `SetMaxNumberOfRuns`, etc. |
| `SchedulerBuilder.cs` | Wraps a `JobBuilder` + `List<TriggerBuilder>`; tracks a `PersistenceBehavior` (`Appended` by default) so persisters know whether to insert / update. |
| `ScheduleOptionsBuilder.cs` | Root config: `_jobMonitor`, `_jobExecutor`, `_jobPersistence`, `_jobClusterServer`, `_jobFactory` slot fields plus public `UnobservedTaskExceptionHandler` and `UseUtcTimestamp`. Internal `_schedulerBuilders` list aggregates everything passed in. |

## For AI Agents

### Working in this directory
- Always go through the static `Create*`/`Period`/`Cron` factories — public ctors are private. This keeps construction chainable and lets the framework swap the concrete builder type.
- The builder-derives-from-domain pattern (`JobBuilder : JobDetail`, `TriggerBuilder : Trigger`) is intentional: callers never have to convert; the scheduler accepts the builder directly. Don't break this by introducing a separate "build → produce immutable" step.
- When wiring custom infrastructure (persistence, monitor, executor, cluster server) into `ScheduleOptionsBuilder`, set the corresponding slot type via the existing fluent methods (e.g. `UseMonitor<T>()` etc.); the host service uses these for DI registration.

### Common patterns
- `sealed partial` split between core API and `*.Setters.cs` for readability.
- Mutator-returns-this for fluent chaining on every public method.

## Dependencies
### Internal
- `Schedule/Details` (`JobDetail`, `Trigger` base types).
- `Schedule/Triggers` (`PeriodTrigger`, `CronTrigger`).
- `Schedule/Persistences` (`PersistenceBehavior`).
- `Poxiao.TimeCrontab` (cron parsing).

### External
- `Microsoft.Extensions.DependencyInjection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
