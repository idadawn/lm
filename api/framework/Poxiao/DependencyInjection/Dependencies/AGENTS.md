<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dependencies

## Purpose
Lifetime marker interfaces consumed by the Poxiao DI scanner. A class that implements `IScoped`, `ITransient`, or `ISingleton` is auto-registered with the corresponding `ServiceLifetime` when `AddDependencyInjection()` runs. `IPrivateDependency` is the hidden base interface that anchors the scan but is not meant to be implemented directly.

## Key Files
| File | Description |
|------|-------------|
| `IPrivateDependency.cs` | Internal root interface; scanner uses `typeof(IPrivateDependency).IsAssignableFrom(t)` as the entry filter. |
| `IScoped.cs` | Marker for `ServiceLifetime.Scoped` registrations (per-request services). |
| `ISingleton.cs` | Marker for `ServiceLifetime.Singleton` (e.g., `SequentialGuidIDGenerator`). |
| `ITransient.cs` | Marker for `ServiceLifetime.Transient`. |

## For AI Agents

### Working in this directory
- These interfaces are intentionally empty and must stay empty — adding members would force every implementer in the codebase to change.
- Application-layer services in `api/src/modularity/**` should pick exactly one lifetime marker; never combine more than one.
- Do not implement `IPrivateDependency` directly; pick one of the three lifetime descendants.

### Common patterns
- Used together with `[Injection]` from `../Attributes/` to refine pattern/named registration.
- `INamedServiceProvider<T>` (`../Providers/`) inspects these markers to honour lifetime-aware resolution.

## Dependencies
### Internal
- Read by `../Extensions/DependencyInjectionServiceCollectionExtensions.cs`.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
