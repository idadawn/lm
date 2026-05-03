<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Providers

## Purpose
Bridges between MVC's discovery pipeline and Poxiao's dynamic-API rules. `DynamicApiControllerFeatureProvider` decides which scanned types become MVC controllers; `MvcActionDescriptorChangeProvider` exposes a `CancellationChangeToken` so MVC re-discovers controllers when modules are loaded/unloaded at runtime.

## Key Files
| File | Description |
|------|-------------|
| `DynamicApiControllerFeatureProvider.cs` | Sealed `ControllerFeatureProvider`; delegates to `Penetrates.IsApiController(typeInfo)`. |
| `MvcActionDescriptorChangeProvider.cs` | `IActionDescriptorChangeProvider` exposing a token cancelled on `NotifyChanges()`. |

## For AI Agents

### Working in this directory
- Both classes are framework-private behaviour — do not derive in feature modules.
- `MvcActionDescriptorChangeProvider` is registered as singleton and consumed by `../Runtimes/DynamicApiRuntimeChangeProvider`. Replacing one without the other will break hot-reload.
- Keep `IsController` decisions inside `Penetrates`; this provider should remain a one-line delegation.

### Common patterns
- Cancellation-token-based change notification (`CancellationTokenSource` recreated on each notify).

## Dependencies
### Internal
- `../Internal/Penetrates`.
### External
- `Microsoft.AspNetCore.Mvc.Controllers`, `Microsoft.AspNetCore.Mvc.Infrastructure`, `Microsoft.Extensions.Primitives`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
