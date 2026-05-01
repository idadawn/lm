<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
`IServiceCollection` / `IServiceProvider` extensions invoked from `Startup.ConfigureServices` and `Startup.Configure` to wire infrastructure that's specific to this host (rather than reusable framework code).

## Key Files
| File | Description |
|------|-------------|
| `SqlSugarConfigureExtensions.cs` | Builds `SqlSugarScope` from `ConnectionStringsOptions`, registers `ISqlSugarClient` (singleton) + `ISqlSugarRepository<>` (scoped) + `SqlSugarUnitOfWork`, applies Oracle column-type fix-ups, sets `OnLogExecuting` / `OnError` AOP for SQL logging via Serilog and MiniProfiler. |
| `OSSServiceConfigureExtensions.cs` | Reads `OssOptions` (provider, endpoint, AK/SK, region, https, cache) and calls `services.AddOSSService` from `OnceMi.AspNetCore.OSS`. |
| `DatabaseInitExtension.cs` | `InitializeLabDatabase` — uses `CodeFirst.InitTables` to create Lab module tables (`ProductSpec`, `AppearanceFeature*`, `RawData*`, `Excel*`, `UnitCategory`, `UnitDefinition`) on a fresh DB and seeds magnetic-property + quantity unit dimensions. |

## For AI Agents

### Working in this directory
- All three extensions are static and live in the `Microsoft.Extensions.DependencyInjection` namespace — keep this convention so call sites in `Startup.cs` stay terse (`services.SqlSugarConfigure()`, `services.OSSServiceConfigure()`).
- `DatabaseInitExtension.InitializeLabDatabase` checks `IsAnyTable("unit_category")` to decide whether to seed; do not add new entities outside the gating `if` block or you'll attempt re-creates on every boot.
- The SqlSugar `EntityService` lambda enforces nullability based on C# nullable reference annotations and translates `long`/`bool` to Oracle `number(...)` — extend here, not in individual entity classes.
- SQL logs go through `Log.Debug` and MiniProfiler — keep them off in production logging filters via `Configurations/Logging.json`.

### Common patterns
- `App.GetOptions<T>()` / `App.GetConfig<T>(section, true)` to read settings.
- Service lifetimes: `Singleton` for clients, `Scoped` for repositories, `Transient` for stateless helpers.
- Seeding pattern: query existence by natural key (`Code` / `Name`), only insert when missing.

## Dependencies
### Internal
- `framework/Poxiao.Extras.DatabaseAccessor.SqlSugar`
- `modularity/lab/Poxiao.Lab.Entity` (entity types for `InitTables`).

### External
- `SqlSugar`, `Mapster`, `OnceMi.AspNetCore.OSS`, `Serilog`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
