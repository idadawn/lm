<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Public entry points for the Poxiao Components feature: extension methods that let host code register a component graph against `IServiceCollection` (DI configuration phase) or `IApplicationBuilder` (middleware pipeline phase). Both walk `[DependsOn]` metadata via `Components/Internal/Penetrates.CreateDependLinkList`, instantiate each component with `Activator.CreateInstance`, and invoke its `Load(...)` method in dependency order.

## Key Files
| File | Description |
|------|-------------|
| `ComponentServiceCollectionExtensions.cs` | `AddComponent<TComponent>` / `AddComponent<TComponent, TOptions>` / `AddComponent(Type, options)` for `IServiceComponent`. Registers components during service-collection build. |
| `ComponentApplicationBuilderExtensions.cs` | `UseComponent<TComponent>` / overloads for `IApplicationComponent`. Hooks components into the middleware pipeline, passing `IWebHostEnvironment`. |

## For AI Agents

### Working in this directory
- All public APIs are `[SuppressSniffer]` static — keep them so they don't leak into Furion/Poxiao API scanners.
- The two extensions deliberately mirror each other (services vs. application builder); changes to one usually need to land in both.
- Don't bypass `Penetrates.CreateDependLinkList` — circular-reference and self-reference checks live there.

### Common patterns
- Generic overload reduces to non-generic `Type` overload (e.g. `AddComponent<T>` → `AddComponent(typeof(T), options)`).
- Component options object is forwarded through `ComponentContext.SetProperty(componentType, options)` on the root context.

## Dependencies
### Internal
- `Components/Internal/Penetrates.cs` — depend-link construction.
- `Components/Contexts/ComponentContext.cs` — passed to each component's `Load`.
### External
- `Microsoft.AspNetCore.Builder`, `Microsoft.AspNetCore.Hosting`, `Microsoft.Extensions.DependencyInjection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
