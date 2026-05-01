<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dependencies

## Purpose
Marker contract layer for the Components feature. Defines the empty `IComponent` interface that all Poxiao framework components derive from (concrete `IServiceComponent` / `IApplicationComponent` live alongside it under `Components/`). The marker lets `AddComponent` / `UseComponent` constrain generics and lets `Penetrates` walk `[DependsOn]` graphs by component type.

## Key Files
| File | Description |
|------|-------------|
| `IComponent.cs` | Empty marker interface `Poxiao.Components.IComponent` — root of the component type hierarchy. |

## For AI Agents

### Working in this directory
- Keep `IComponent` empty — it is intentionally a marker. Behavioural contracts belong on `IServiceComponent` / `IApplicationComponent`, not here.
- Any new component-marker interface should also live in `Poxiao.Components` namespace and be derived from `IComponent` so the dependency walker accepts it.

### Common patterns
- Marker-interface pattern used elsewhere in the framework (e.g. `ITransient`, `IConfigurableOptions`) — DI/feature wiring discovers types by interface assignability.

## Dependencies
### Internal
- Consumed by `Components/Extensions/*Extensions.cs` for generic constraints.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
