<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Common.Core

## Purpose
Runtime services for the modular monolith — anything that needs project-references on `Systems.Entitys`, `Message.Entitys`, `TaskScheduler.Entitys`, `VisualDev.Entitys`. Hosts the EventBus subscribers (logging + user-sync via RabbitMQ), the global MVC filters that publish those events, the IM WebSocket handler, the dynamic Quartz-style Job runtime, the Manager abstractions (DataBase, Files, InfluxDB, User), and the Mapster registrations.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Common.Core.csproj` | Sets `PreserveCompilationContext=true`. References `Authentication.JwtBearer`, `EventBus.RabbitMQ`, `WebSockets`, plus `Systems`/`Message`/`TaskScheduler`/`VisualDev` entitys. Pulls in InfluxData.Net, DotNetCore.Natasha.CSharp (for dynamic compile), Microsoft.CodeAnalysis.CSharp. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `EventBus/` | LogEventSubscriber, UserEventSubscriber, RabbitMQ storer, EventBus options (see `EventBus/AGENTS.md`) |
| `Filter/` | `LogExceptionHandler` + `RequestActionFilter` — global MVC filters (see `Filter/AGENTS.md`) |
| `Handlers/` | `IMHandler` — instant-messaging WebSocket handler (see `Handlers/AGENTS.md`) |
| `Job/` | Dynamic Quartz-style scheduler — `DbJobPersistence`, `DynamicJobCompiler`, `PoxiaoHttpJob` (see `Job/AGENTS.md`) |
| `Manager/` | Service abstractions: DB / Files / InfluxDB / User (see `Manager/AGENTS.md`) |
| `Mapper/` | Mapster `IRegister` for cross-DTO mapping (see `Mapper/AGENTS.md`) |

## For AI Agents

### Working in this directory
- This project is the only one in `common/` that is allowed to depend on feature-module entitys; preserve that direction (no module should reference back into Core).
- `_sqlSugarClient` is held as a `static SqlSugarScope?` in event subscribers/handlers so that tenant-switching `ChangeDatabase(...)` and `AddConnection(...)` carry across calls in the same request scope.
- Most multi-tenant code follows the same shape: read `KeyVariable.MultiTenancy`, look up `GlobalTenantCacheModel` from the `CommonConst.GLOBALTENANT` cache, and either set an AOP override (`COLUMN` mode) or `AddConnection` + `ChangeDatabase` (database-per-tenant mode). When you add a new component touching tenants, copy this exact pattern.

### Common patterns
- DI lifetimes via interfaces: `IEventSubscriber, ISingleton`, `IGlobalExceptionHandler, ISingleton`.
- Scoped helpers grab a fresh `IServiceScope` to read the cache when running outside a request (`using var serviceScope = _serviceProvider.CreateScope();`).

## Dependencies
### Internal
- All four entitys projects under `system/`, `message/`, `taskschedule/`, `visualdev/`, plus `Poxiao.Common`.

### External
- InfluxData.Net, RabbitMQ.Client, DotNetCore.Natasha.CSharp, Microsoft.CodeAnalysis.CSharp, SkiaSharp.NativeAssets.Linux.NoDependencies.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
