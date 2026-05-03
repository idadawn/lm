<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Lab

## Purpose
Implementation assembly for the lab module: hosts service classes, helpers, event-bus subscribers, and DI extensions that drive the **导入 → RawData → IntermediateData → 公式计算 → 判定** pipeline. Pairs with `Poxiao.Lab.Entity` (entities/DTOs) and `Poxiao.Lab.Interfaces` (contracts).

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Lab.csproj` | .NET 10 project, references `Poxiao.Lab.Entity`, `Poxiao.Lab.Interfaces`, infrastructure framework. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `EventBus/` | Event source + subscriber for `IntermediateData:CalcByBatch` (see `EventBus/AGENTS.md`). |
| `Extensions/` | Repository/service extension methods (see `Extensions/AGENTS.md`). |
| `Helpers/` | Pure-logic helpers: `FormulaParser`, `IntermediateDataGenerator`, `FurnaceNoHelper`, `RangeFormulaCalculator` (see `Helpers/AGENTS.md`). |
| `Interfaces/` | Module-internal interfaces such as `IIntermediateDataColorService` (see `Interfaces/AGENTS.md`). |
| `Service/` | Business services: `RawDataImportSessionService`, `IntermediateDataFormulaBatchCalculator`, `DashboardService`, `MonthlyQualityReportService`, `ProductSpecService`, magnetic/appearance services, RabbitMQ consumer/publisher (see `Service/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Service classes implement interfaces declared in `Poxiao.Lab.Interfaces` (and a few module-local ones in `Interfaces/`); register via `ITransient`/`ISingleton` marker interfaces — DI is convention-based.
- Heavy/long-running calculation logic should remain pure (no `IHttpContextAccessor`/`ICacheManager`) so it can also run inside the Worker process.
- New helpers belong under `Helpers/`; new event subscribers under `EventBus/`.

### Common patterns
- `Poxiao.Lab.Service` namespace is shared across files (avoid duplicate class names).
- Logging via `Poxiao.Logging.Log`, repositories via `ISqlSugarRepository<T>`.
- Calculation entry points: `IntermediateDataFormulaBatchCalculator.CalculateByBatchAsync` triggered from `IntermediateDataCalcEventSubscriber` or RabbitMQ `CalcProgressConsumer`.

## Dependencies
### Internal
- `Poxiao.Lab.Entity` (entities, DTOs, attributes, enums, config).
- `Poxiao.Lab.Interfaces` and `Poxiao.Lab.Service` (interface project).
- `framework/Poxiao` (DI, EventBus, SqlSugar, FriendlyException, DynamicApiController).
### External
- SqlSugar, Mapster, MiniExcel, Newtonsoft.Json.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
