<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
The single entry point that ties Poxiao's DI conventions into ASP.NET Core's `IServiceCollection`. `AddDependencyInjection()` binds `DependencyInjectionSettingsOptions`, scans `App.EffectiveTypes` for `IPrivateDependency` implementers, and registers each with the right lifetime, pattern, and optional `AspectDispatchProxy`. Also exposes `AddDispatchProxyForInterface` for AOP-style interception.

## Key Files
| File | Description |
|------|-------------|
| `DependencyInjectionServiceCollectionExtensions.cs` | `AddDependencyInjection`, `AddDispatchProxyForInterface<TProxy,TI>`, internal `AddInnerDependencyInjection`, lifetime/pattern resolution helpers. |

## For AI Agents

### Working in this directory
- Called once from `Poxiao.API.Entry` startup — do not call from feature modules.
- Class lives in the `Microsoft.Extensions.DependencyInjection` namespace (intentional, so consumers don't need an extra `using`).
- New scanning rules belong here, not scattered across modules; preserve ordering by `[Injection].Order`.

### Common patterns
- `App.EffectiveTypes` / `App.Assemblies` — Furion-style global type cache, not LINQ over `AppDomain`.
- Uses `ServiceDescriptor.Describe(...)` plus `services.Add` / `services.TryAdd...` based on `InjectionActions`.
- Proxy registration goes through `AspectDispatchProxy` from `Poxiao` core.

## Dependencies
### Internal
- `Poxiao` core (`App`), `Poxiao.DynamicApiController` (proxy-aware controllers), `Poxiao.Reflection`.
### External
- `Microsoft.Extensions.DependencyInjection.Extensions`, `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
