<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Lab.CalcWorker

## Purpose
Standalone .NET 10 Worker Service (`Microsoft.NET.Sdk.Worker`) that off-loads laboratory data calculation/judgement work from the API host. Subscribes to RabbitMQ queues `lab.calc.task` (basic + formula calc) and `lab.judge.task` (magnetic / standard judgement), runs SqlSugar-backed batch calculators, and publishes progress to `lab.calc.progress`. Deliberately bypasses Furion's `Inject()` to avoid Web service auto-discovery — uses raw `Host.CreateDefaultBuilder` + Serilog.

## Key Files
| File | Description |
|------|-------------|
| `Program.cs` | Top-level program — Serilog console + `logs/calc-worker-.log`, loads every JSON in `Configurations/`, binds `RabbitMqOptions` and `LabOptions`, registers `AddSqlSugarForWorker` + `AddLabCalculationServices`, hosts `CalcTaskConsumer` and `JudgeTaskConsumer`. |
| `Poxiao.Lab.CalcWorker.csproj` | Worker SDK; refs `RabbitMQ.Client` 6.8.1, `Serilog.Sinks.Console`/`File`, `Poxiao.Lab` and the SqlSugar accessor. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Configurations/` | `appsettings.json` with RabbitMQ + Lab + ConnectionStrings. See `Configurations/AGENTS.md`. |
| `Extensions/` | `AddSqlSugarForWorker`, `AddLabCalculationServices`. See `Extensions/AGENTS.md`. |
| `Repositories/` | `WorkerSqlSugarRepository<T>` lightweight repo (no multi-tenant). See `Repositories/AGENTS.md`. |
| `Services/` | RabbitMQ options, progress publisher, batch tracker, worker formula service. See `Services/AGENTS.md`. |
| `Worker/` | `BackgroundService` consumers — `CalcTaskConsumer`, `JudgeTaskConsumer`. See `Worker/AGENTS.md`. |

## For AI Agents

### Working in this directory
- This process must NOT depend on `App.GetConfig`, `ICacheManager`, multi-tenant context, or anything from `Poxiao.Apps` / `Poxiao.System` — those require Furion's web bootstrap. Always inject options via `IOptions<T>` and use the worker-specific repo / SqlSugar registration.
- Connection strings here are read from `ConnectionStrings:Default` (a single string), not from the array shape used by the API host. Don't confuse the two.
- Progress messages mirror the format consumed by `CalcProgressConsumer` in the API host — keep `BatchId`, `Total`, `Completed`, `SuccessCount`, `FailedCount`, `Status` (`PROCESSING` / `COMPLETED` / `FAILED`) stable.
- Calculation logic lives in `Poxiao.Lab.Service.IntermediateDataFormulaBatchCalculator` / `IntermediateDataGenerator`; the worker only orchestrates and reports.

### Common patterns
- Per-item mode (single `IntermediateDataId`) uses `BatchProgressTracker` + `SemaphoreSlim` for bounded concurrency, with CAS-protected "last completer triggers formula calc".
- Batch mode (`IntermediateDataIds` list / no item id) processes synchronously per message.
- All `IModel` operations (Ack/Publish) hold a `lock(channelLock)` because RabbitMQ.Client `IModel` is not thread-safe.

## Dependencies
### Internal
- `framework/Poxiao.Extras.DatabaseAccessor.SqlSugar`
- `modularity/lab/Poxiao.Lab` (calculator + formula helpers).

### External
- `RabbitMQ.Client` 6.8.1, `Serilog`, `Microsoft.Extensions.Hosting` 10.0.0-preview.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
