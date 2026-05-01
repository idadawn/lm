<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# EventBus

## Purpose
Event-driven side effects: log persistence, user-info synchronisation, RabbitMQ-backed source storer, and the retry policy. The MVC layer publishes `LogEventSource`/`UserEventSource` instances and these subscribers persist them in the correct tenant database.

## Key Files
| File | Description |
|------|-------------|
| `EventBusOptions.cs` | `IConfigurableOptions`. Selects backend via `EventBusType` enum (`Memory` / `RabbitMQ` / `Redis` / `Kafka`); HostName/UserName/Password for RabbitMQ. |
| `LogEventSubscriber.cs` | `ISingleton`. Subscribes to `Log:CreateReLog`/`CreateExLog`/`CreateVisLog`/`CreateOpLog`. Switches `_sqlSugarClient` to the tenant DB (column or db-per-tenant) before `Insertable(log.Entity)`. |
| `UserEventSubscriber.cs` | `ISingleton`. Subscribes to `User:UpdateUserLogin` (write last-login fields) and `User:Maxkey_Identity` (Maxkey SSO sync — CREATE/UPDATE/DELETE/PASSWORD actions, default password from `CommonConst.DEFAULTPASSWORD`). |
| `RetryEventHandlerExecutor.cs` | Custom executor that wraps subscriber invocation with retry semantics. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Sources/` | `LogEventSource` / `UserEventSource` — payload carriers (see `Sources/AGENTS.md`) |
| `Storers/` | `RabbitMQEventSourceStorer` — bounded `Channel` bridge (see `Storers/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Subscribers are singletons but cache `_sqlSugarClient` as a static field. The tenant-switching dance (`AddConnection` + `ChangeDatabase`, or AOP DataExecuting override for `COLUMN` mode) is the only safe shape — copy verbatim when adding new subscribers.
- Event ids follow `Module:Action` (`Log:CreateReLog`, `User:UpdateUserLogin`). Don't break this convention; the IM/MQ subscribers route on prefixes.

### Common patterns
- Tenant lookup: `await _cacheManager.GetAsync<List<GlobalTenantCacheModel>>(CommonConst.GLOBALTENANT)` then `.Find(tenant => tenant.TenantId == log.TenantId)`.

## Dependencies
### Internal
- `Poxiao.EventBus`, `Poxiao.Infrastructure.Manager.ICacheManager`, `Poxiao.Systems.Entitys.System.SysLogEntity`, `Poxiao.Systems.Entitys.Permission.UserEntity` (+UserRelationEntity, OrganizeEntity).

### External
- RabbitMQ.Client.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
