<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Worker-only `IServiceCollection` extensions. Replace the framework's web-centric SqlSugar registration and the `ITransient`/`ITransientDependency` auto-scan with explicit, lightweight registrations that work in a Generic Host worker.

## Key Files
| File | Description |
|------|-------------|
| `SqlSugarExtensions.cs` | `AddSqlSugarForWorker` — reads `ConnectionStrings:Default`, builds a single-config `SqlSugarScope` (MySQL, auto-close, nullable-aware `EntityService`), registers `ISqlSugarClient` (singleton) and `ISqlSugarRepository<>` → `WorkerSqlSugarRepository<>` (scoped). Skips multi-tenant, cache, and `App.GetConfig` dependencies. |
| `LabServiceExtensions.cs` | `AddLabCalculationServices` — manually registers `IFormulaParser` → `FormulaParser`, `IIntermediateDataFormulaService` → `WorkerIntermediateDataFormulaService` (worker variant), `IntermediateDataFormulaBatchCalculator`, and `IntermediateDataGenerator` as transient. |

## For AI Agents

### Working in this directory
- Adding a new Lab calculation collaborator: register it here as Transient and reference the type from `Poxiao.Lab.Service` / `Poxiao.Lab.Helpers`.
- Do NOT register `IIntermediateDataFormulaService` against the API-side implementation; the worker uses a stripped-down service that only implements `GetListAsync`.
- The SqlSugar registration here intentionally diverges from `Poxiao.API.Entry/Extensions/SqlSugarConfigureExtensions.cs` (single-string vs. `ConnectionConfigs` array) — keep them in sync logically (DB type, AOP) without copying the API code.
- Logging AOP for SQL is currently commented out; uncomment cautiously to avoid log floods during batch calc.

### Common patterns
- Static class in `Poxiao.Lab.CalcWorker.Extensions` namespace, fluent `IServiceCollection` return.
- Manual interface→implementation mapping (no DI conventions).

## Dependencies
### Internal
- `../Repositories/WorkerSqlSugarRepository.cs`
- `../Services/WorkerIntermediateDataFormulaService.cs`
- `modularity/lab/Poxiao.Lab` (`IFormulaParser`, `FormulaParser`, `IntermediateDataFormulaBatchCalculator`, `IntermediateDataGenerator`).

### External
- `SqlSugar`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
