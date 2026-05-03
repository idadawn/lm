<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
`IServiceCollection` / `IMvcBuilder` extensions that wire dynamic API into the ASP.NET Core pipeline: registers the application-model convention, the feature provider, the runtime change provider and the `text/plain` formatter, and adds every loaded assembly to the `ApplicationPartManager` so controllers from any module are discoverable.

## Key Files
| File | Description |
|------|-------------|
| `DynamicApiControllerServiceCollectionExtensions.cs` | `AddDynamicApiControllers(this IMvcBuilder)` and `AddDynamicApiControllers(this IServiceCollection)` overloads. |

## For AI Agents

### Working in this directory
- Must be called after `AddControllers()`; the implementation throws `InvalidOperationException` otherwise — preserve that guard.
- Lives in the `Microsoft.Extensions.DependencyInjection` namespace so consumers don't need extra usings.
- Adds every assembly in `App.Assemblies` to `ApplicationPartManager` to support `<Project Sdk="Microsoft.NET.Sdk">` (non-Web) modules.

### Common patterns
- Registers `MvcActionDescriptorChangeProvider` as singleton and `IDynamicApiRuntimeChangeProvider` as scoped/singleton (see `../Providers/` and `../Runtimes/`).

## Dependencies
### Internal
- `Poxiao` core, sibling sub-folders.
### External
- `Microsoft.AspNetCore.Mvc`, `Microsoft.AspNetCore.Mvc.ApplicationParts`, `Microsoft.AspNetCore.Mvc.Formatters`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
