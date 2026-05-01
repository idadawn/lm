<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Worker

## Purpose
RabbitMQ-driven `BackgroundService` consumers that drive the laboratory calculation pipeline. Two queues are listened to in parallel by two independent hosted services so calc throughput does not block judgement throughput.

## Key Files
| File | Description |
|------|-------------|
| `CalcTaskConsumer.cs` | Listens on `lab.calc.task`. Three modes: per-item (`IntermediateDataId` set) with `SemaphoreSlim`-bounded concurrency, batch-by-IDs (`IntermediateDataIds` list) for manual recalc, and legacy whole-batch (`BatchId` only). Per-item path uses `BatchProgressTracker.TryMarkAllCompleted` (CAS) so the last completer triggers `IntermediateDataFormulaBatchCalculator.CalculateByBatchAsync` followed by chunked auto-judgement (`JudgeByIdsAsync`, chunk size 50). Manual `BasicAck` under `channelLock`. Reconnects with 5s backoff on exception. |
| `JudgeTaskConsumer.cs` | Listens on `lab.judge.task`. Per-item only, two task types: `MAGNETIC_JUDGE` (locate intermediate data by furnace number, refresh magnetic fields, judge) and `JUDGE` (judge a single `IntermediateDataId`, aggregate progress by `BatchId`). Same connection / Ack pattern as `CalcTaskConsumer`. |

## For AI Agents

### Working in this directory
- `IModel` is not thread-safe; every `BasicAck` / `BasicPublish` site holds `lock(channelLock)`. Keep this discipline when adding new ack paths.
- Set `DispatchConsumersAsync = true` on the `ConnectionFactory` — handlers are async lambdas. Without this flag, async exceptions silently disappear.
- Never resolve scoped services (`ISqlSugarRepository<>`, generators) directly from `_serviceProvider`. Always wrap in `using var scope = _serviceProvider.CreateScope()` per message.
- Progress messages must include `BatchId`, `Total`, `Completed`, `SuccessCount`, `FailedCount`, `Status` — the API host's `CalcProgressConsumer` and the frontend WebSocket bridge depend on this exact shape.
- Failure path: on `Exception`, the consumer catches, logs, BUT still calls `BasicAck` to drop the poison message. If you change to `BasicNack(requeue: true)`, ensure batch state isn't double-counted via `BatchProgressTracker`.
- The 1s `await Task.Delay(1000, stoppingToken)` at start of `ExecuteAsync` exists so the Generic Host can finish wiring services before the first DB call — leave it alone.

### Common patterns
- One consumer per queue, both inheriting `BackgroundService`.
- `factory.DispatchConsumersAsync = true` + `AsyncEventingBasicConsumer`.
- `Channel.BasicQos(0, prefetchCount, global:false)` with prefetchCount ≥ MaxConcurrency.

## Dependencies
### Internal
- `../Services/CalcProgressPublisher.cs`, `../Services/BatchProgressTracker.cs`, `../Services/RabbitMqOptions.cs`
- `modularity/lab/Poxiao.Lab.Service` (calculator), `modularity/lab/Poxiao.Lab.Entity` (entities, DTOs, enums), `modularity/lab/Poxiao.Lab.Helpers`.

### External
- `RabbitMQ.Client` (`AsyncEventingBasicConsumer`, `IModel`), `SqlSugar`, `System.Text.Json`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
