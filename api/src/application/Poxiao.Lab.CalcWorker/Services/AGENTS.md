<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Services

## Purpose
Worker-side support services for the laboratory calculation pipeline: RabbitMQ options, the progress publisher, the in-memory batch progress tracker, and a stripped-down formula data service used only by the worker.

## Key Files
| File | Description |
|------|-------------|
| `RabbitMqOptions.cs` | POCO bound to the `RabbitMQ` config section. Properties: `HostName`, `Port` (5672), `UserName`/`Password`, `TaskQueueName=lab.calc.task`, `JudgeQueueName=lab.judge.task`, `ProgressQueueName=lab.calc.progress`, `PrefetchCount=20`, `MaxConcurrency=20`, `ProgressInterval=10`. |
| `CalcProgressPublisher.cs` | Singleton, lazy-connecting publisher to `lab.calc.progress`. Declares the queue durable, reuses one `IConnection`/`IModel`, serialises payload as JSON, sets `Persistent=true` on basic properties. Implements `IDisposable`. |
| `BatchProgressTracker.cs` | `ConcurrentDictionary<string, BatchState>` with `Interlocked` counters. `GetOrCreate` seeds state from the first per-item message; `ReportCompleted` increments success/failed/total; `TryMarkAllCompleted` uses CAS so exactly one thread triggers post-batch formula calc. `BatchState` exposes raw fields for `Volatile.Read`/`Interlocked.Increment`. |
| `WorkerIntermediateDataFormulaService.cs` | Implements `IIntermediateDataFormulaService.GetListAsync` only — queries `IntermediateDataFormulaEntity`, manually maps to DTO so the `FormulaType` enum becomes its name (`"CALC"`) instead of an integer string (avoids downstream `NormalizeFormulaType` filter dropping all rows). All other interface members throw / are unused inside the worker. |

## For AI Agents

### Working in this directory
- `BatchProgressTracker.BatchState` deliberately uses public mutable fields (not properties) — they are accessed via `Interlocked.Increment(ref ...)` which only works on fields. Don't refactor to auto-properties.
- The progress publisher creates the queue with `durable: true` — keep this in sync with whatever creates the queue on the API side; mismatched flags cause `PRECONDITION_FAILED`.
- The mapping workaround in `WorkerIntermediateDataFormulaService.GetListAsync` is load-bearing; if you switch to `Select<DTO>()` projection, formula filtering will silently drop everything. Document any change in `IFormulaParser` / formula-type normalisation jointly with `Poxiao.Lab`.
- Inside batch processing, always create `using var scope = _serviceProvider.CreateScope()` for DB work — do not resolve scoped services from the singleton publisher / tracker.

### Common patterns
- `IOptions<RabbitMqOptions>` injection.
- `Volatile.Read` for snapshot reads of mutable counters; `Interlocked.CompareExchange` for one-shot triggers.

## Dependencies
### Internal
- `modularity/lab/Poxiao.Lab.Interfaces`, `modularity/lab/Poxiao.Lab.Entity` (formula DTO/entity).

### External
- `RabbitMQ.Client` (publisher), `System.Text.Json`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
