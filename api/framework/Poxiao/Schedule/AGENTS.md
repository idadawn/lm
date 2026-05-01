<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Schedule

## Purpose
In-process job scheduler: cron + interval triggers, persistence, dashboard, clustering, monitoring and HTTP control surface. Jobs are classes that implement `IJob` (declared with `[JobDetail]`) and carry one or more `[Cron]` / `[Period]` / macro (`[Daily]`, `[Hourly]`, `[Workday]`, …) trigger attributes; `services.AddSchedule(...)` discovers them and runs them via `ScheduleHostedService`.

## Key Files
| File | Description |
|------|-------------|
| `Schedular.cs` | Two static façades: `ScheduleServe.Run(...)` boots an isolated scheduler from a `ScheduleOptionsBuilder` (for non-DI hosts); `Schedular` (typo retained) provides ambient resolution helpers from inside running jobs. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[JobDetail]`, `[Trigger]`, `[Cron]`, `[Period]` and the macro / period-unit subfamilies (see `Attributes/AGENTS.md`) |
| `Builders/` | Fluent `JobBuilder`, `TriggerBuilder`, `SchedulerBuilder`, `ScheduleOptionsBuilder` (see `Builders/AGENTS.md`) |
| `Constants/` | Scheduling constants |
| `Contexts/` | `JobExecutingContext` and surrounding execution-time context types |
| `Converters/` | JSON converters for triggers / job details |
| `Dashboard/` | Built-in HTTP dashboard endpoints |
| `Dependencies/` | DI marker interfaces for jobs / monitors / persisters |
| `Details/` | `JobDetail`, `Trigger` value objects |
| `Events/` | Job lifecycle event args |
| `Executors/` | `IJobExecutor` strategies (parallel / serial / custom) |
| `Extensions/` | `IServiceCollection.AddSchedule(...)` and helpers |
| `Factories/` | Job instance factories + scheduler factory |
| `HostedServices/` | `ScheduleHostedService` — the long-running `IHostedService` that ticks the scheduler |
| `Http/` | HTTP API surface for cluster / external control |
| `Internal/` | Internal helpers, locks, runtime state |
| `Loggers/` | Scoped logger wrappers |
| `Monitors/` | `IJobMonitor` hooks (before/after execution) |
| `Persistences/` | `IJobPersistence` to save scheduler state across restarts |
| `Schedulers/` | `IScheduler` core implementation |
| `Servers/` | Cluster server abstractions |
| `Triggers/` | `Trigger`, `CronTrigger`, `PeriodTrigger` runtime types |

## For AI Agents

### Working in this directory
- New job: `[JobDetail("foo")]` + `[Cron("0/5 * * * * ?")]` (or `[Daily]`, `[PeriodSeconds(10)]` etc.) on a class implementing `IJob`. Don't manually `new` jobs — let the factory / DI resolve them so scoped services work.
- Cron expressions go through `Poxiao.TimeCrontab` (a vendored library); use `CronStringFormat` to pick standard / quartz / `@`-macro modes.
- `Concurrent = false` on `[JobDetail]` switches to serial execution; combine with `MaxNumberOfRuns` / `MaxNumberOfErrors` on the trigger for backpressure.

### Common patterns
- Attribute-driven discovery + builder-based programmatic config (both APIs go through the same `JobDetail` / `Trigger` value types).
- `Schedular.cs` deliberately misspells "Scheduler" — preserved across the codebase for backward compat with namespace `Poxiao.Schedule`.

## Dependencies
### Internal
- `Poxiao.TimeCrontab` (cron parsing).
- `Poxiao.Options`, `Poxiao.Reflection`, the standard Poxiao DI / hosting plumbing.

### External
- `Microsoft.Extensions.Hosting` (`IHostedService`), `Microsoft.Extensions.DependencyInjection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
