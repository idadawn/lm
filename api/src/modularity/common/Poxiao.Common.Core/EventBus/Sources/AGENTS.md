<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Sources

## Purpose
`IEventSource` payload carriers — the data shape published to and consumed from the EventBus. Each source bundles a tenant id with the entity to persist so that subscribers can route to the correct database.

## Key Files
| File | Description |
|------|-------------|
| `LogEventSource.cs` | Carries `EventId`, `TenantId`, and a `SysLogEntity`. Used for the `Log:Create{Re,Ex,Vis,Op}Log` ids by `RequestActionFilter` / `LogExceptionHandler`. |
| `UserEventSource.cs` | Carries `EventId`, `TenantId`, and a `UserEntity` (or string payload for Maxkey JSON). Used by `User:UpdateUserLogin` / `User:Maxkey_Identity`. |

## For AI Agents

### Working in this directory
- Implement `IEventSource` and expose: `string EventId { get; }`, `object Payload { get; }`, `CancellationToken CancellationToken { get; }`, `DateTime CreatedTime { get; }`. Constructors should set the entity reference + tenant id.
- Don't add behaviour here; subscribers handle persistence.

### Common patterns
- `CreatedTime = DateTime.UtcNow` (subscribers can convert to local time when persisting).

## Dependencies
### Internal
- `Poxiao.EventBus.IEventSource`, `Poxiao.Systems.Entitys.System.SysLogEntity`, `Poxiao.Systems.Entitys.Permission.UserEntity`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
