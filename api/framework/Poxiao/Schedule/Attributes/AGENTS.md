<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
Declarative configuration for jobs and triggers. `[JobDetail]` describes the job (id, group, concurrency, description); `[Trigger]` is the abstract base for all firing-rule attributes; `[Cron]` and `[Period]` are the concrete primitives; macros (`[Daily]`, `[Hourly]`, `[Workday]`, …) and at-attributes (`[DailyAt]`, `[HourlyAt]`, …) and period-unit attributes (`[PeriodSeconds]`, …) are convenience derivatives.

## Key Files
| File | Description |
|------|-------------|
| `JobDetailAttribute.cs` | `[JobDetail(jobId, [concurrent], [description])]` — sealed; properties: `JobId`, `GroupName`, `Description`, `Concurrent` (default `true`). |
| `TriggerAttribute.cs` | Base class for trigger attributes — `TriggerId`, `Description`, `StartTime`/`EndTime` (parsed into `RuntimeStartTime`/`EndTime`), `MaxNumberOfRuns`, `MaxNumberOfErrors`, `NumRetries`, `RetryTimeout` (1000ms default), `StartNow`, `RunOnStart`, `ResetOnlyOnce`. Holds `RuntimeTriggerType` + `RuntimeTriggerArgs` consumed by the scheduler factory. |
| `CronAttribute.cs` | `[Cron("expr", CronStringFormat)]` — wraps `Poxiao.TimeCrontab` cron parsing; binds to `CronTrigger`. |
| `PeriodAttribute.cs` | `[Period(intervalMs)]` — millisecond-interval trigger; binds to `PeriodTrigger`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `MacroAts/` | `@daily/@hourly/...` cron macros that take field arguments (e.g. `[DailyAt(2, 30)]`) (see `MacroAts/AGENTS.md`) |
| `Macros/` | Bare cron macros without arguments — `[Daily]`, `[Hourly]`, `[Workday]`, ... (see `Macros/AGENTS.md`) |
| `Periods/` | Convenience period-unit subclasses (`[PeriodSeconds]`, `[PeriodMinutes]`, `[PeriodHours]`) (see `Periods/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `AllowMultiple = true` on `[Trigger]`, `[Cron]`, `[Period]`: a single job class can carry many trigger attributes and the scheduler creates one trigger per attribute instance. Use this for "fire at 09:00 and 17:00" style.
- `StartNow = true` (default) means the trigger is enabled the moment the scheduler starts; flip to `false` for triggers that need explicit programmatic enablement.
- Don't set `StartTime` / `EndTime` to invalid date strings — the setters call `Convert.ToDateTime` and will throw at attribute construction.

### Common patterns
- Attribute → Runtime trigger: `RuntimeTriggerType` + `RuntimeTriggerArgs` are populated in the base ctor and read by the scheduler factory to construct the matching `Trigger` subclass without re-parsing.

## Dependencies
### Internal
- `Poxiao.TimeCrontab` for `CronStringFormat`.
- `Schedule/Triggers` for the runtime `CronTrigger`, `PeriodTrigger` types.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
