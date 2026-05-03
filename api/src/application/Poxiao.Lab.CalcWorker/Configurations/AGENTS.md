<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Configurations

## Purpose
Worker-process configuration. Every `*.json` in this directory is loaded by `Program.cs` into the `IConfiguration` root, so settings can be split if needed; today only `appsettings.json` exists.

## Key Files
| File | Description |
|------|-------------|
| `appsettings.json` | `Logging` (Default: Information, `Poxiao.Lab.CalcWorker`: Debug); `RabbitMQ` (host/port/creds, `TaskQueueName=lab.calc.task`, `JudgeQueueName=lab.judge.task`, `ProgressQueueName=lab.calc.progress`, `PrefetchCount=20`, `MaxConcurrency=20`, `ProgressInterval=10`); `ConnectionStrings.Default` (MySQL `lumei`); `Lab.Formula` precision options (`EnablePrecisionAdjustment`, `DefaultPrecision`, `MaxPrecision`). |

## For AI Agents

### Working in this directory
- `RabbitMQ` section keys must match the property names on `Services.RabbitMqOptions` exactly (case sensitive).
- Connection string shape is the single-string MySQL form (no `{0}` placeholder, no `ConnectionConfigs` array) because the worker uses `AddSqlSugarForWorker` not `SqlSugarConfigure`.
- `Lab` section is bound to the same `Poxiao.Lab.Entity.Config.LabOptions` used by the API host — keep precision values identical between processes to avoid divergent calc results.
- `ProgressInterval` controls how often per-item mode emits progress to `lab.calc.progress`; raising it reduces frontend chatter but slows perceived progress bars.

## Dependencies
### Internal
- Bound by `../Program.cs` to `RabbitMqOptions` (`Services/RabbitMqOptions.cs`) and `LabOptions` (`modularity/lab/Poxiao.Lab.Entity/Config/LabOptions.cs`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
