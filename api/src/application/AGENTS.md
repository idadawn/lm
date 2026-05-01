<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# application

## Purpose
Container for runnable host projects. Two processes are produced from this directory: `Poxiao.API.Entry` (the ASP.NET Core web API exposing all module controllers) and `Poxiao.Lab.CalcWorker` (a `Microsoft.NET.Sdk.Worker` background service that consumes the `lab.calc.task`/`lab.judge.task` RabbitMQ queues for long-running formula calculation and judgement).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.API.Entry/` | Web API host — `Program.cs` (Serve.Run + WebComponent), `Startup.cs`, `Configurations/`, `Extensions/`, JWT handler (see `Poxiao.API.Entry/AGENTS.md`). |
| `Poxiao.Lab.CalcWorker/` | RabbitMQ-driven calculation/judgement Worker — `CalcTaskConsumer`, `JudgeTaskConsumer`, batch progress tracker, manual SqlSugar wiring (see `Poxiao.Lab.CalcWorker/AGENTS.md`). |

## For AI Agents

### Working in this directory
- The two hosts deliberately bootstrap differently: API uses the Poxiao framework (`Serve.Run`, auto-discovered components); the Worker avoids `Inject()` and uses raw `Host.CreateDefaultBuilder` + manual SqlSugar registration (see comments in `Poxiao.Lab.CalcWorker/Program.cs`). Do not blindly unify the two — the Worker would otherwise pull in unwanted web service discovery and exit.
- Both hosts share the same Lab module project reference, so SqlSugar entities and helpers (`IntermediateDataGenerator`, `IntermediateDataFormulaBatchCalculator`) must remain compatible with both lifetimes.
- RabbitMQ host/credentials come from `EventBusOptions` in the API and from `RabbitMqOptions` (`RabbitMQ` config section) in the Worker — keep these aligned when changing infra.

### Common patterns
- Configuration is split into many small JSON files under `<host>/Configurations/` and merged at startup by Poxiao (API) or by an explicit `ConfigureAppConfiguration` loop (Worker).
- Health check at `/health` is exposed by the API for Docker probes.

## Dependencies
### Internal
- `../modularity/lab/Poxiao.Lab` (consumed by both hosts), all other modules under `../modularity/*` (API only).
- `../../framework/Poxiao.Extras.DatabaseAccessor.SqlSugar`.

### External
- `RabbitMQ.Client` 6.8.1, Serilog (Worker), `IGeekFan.AspNetCore.Knife4jUI` (API Swagger UI under `/newapi`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
