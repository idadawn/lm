<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# EventBus

## Purpose
In-process event source + subscriber that fires the public `IntermediateData:CalcByBatch` event after a raw-data import session completes. Decouples `RawDataImportSessionService.CompleteImport` from the heavy formula-calculation pipeline (`IntermediateDataFormulaBatchCalculator`). Note: judgement (JUDGE) execution has been migrated to the RabbitMQ Worker; this in-process path is retained for batch CALC compatibility.

## Key Files
| File | Description |
|------|-------------|
| `IntermediateDataCalcEventSource.cs` | `IEventSource` carrying `TenantId`, `SessionId`, `BatchId`, `DataCount`, `UnitPrecisions` (Dictionary<string, UnitPrecisionInfo>). |
| `IntermediateDataCalcEventSubscriber.cs` | Singleton subscriber `[EventSubscribe("IntermediateData:CalcByBatch")]` — resolves `IntermediateDataFormulaBatchCalculator` from a scoped provider and calls `CalculateByBatchAsync(batchId, unitPrecisions)`. |

## For AI Agents

### Working in this directory
- The publisher lives in `Service/RawDataImportSessionService.CompleteImport` — keep both publish payload and subscriber payload in sync if you add fields to the event source.
- Subscriber handlers must be idempotent on `BatchId` (early-return when blank); failures are logged via `Poxiao.Logging.Log.Error` but not rethrown so the event bus does not retry blindly.
- Subscriber is a singleton, so create a DI scope inside the handler (`_serviceProvider.CreateScope()`) before resolving scoped services.

### Common patterns
- Event ID strings are namespaced `"IntermediateData:<Action>"`.
- `UnitPrecisions` carries per-field `UnitId` + `DecimalPlaces` from the import template so the calculator can apply unit conversion + rounding.

## Dependencies
### Internal
- `Poxiao.EventBus` (framework abstractions: `IEventSource`, `IEventSubscriber`, `EventHandlerExecutingContext`).
- `Poxiao.Lab.Entity.Config.UnitPrecisionInfo`.
- `Poxiao.Lab.Service.IntermediateDataFormulaBatchCalculator`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
