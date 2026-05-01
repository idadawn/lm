<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Runtimes

## Purpose
Hot-add / hot-remove of assemblies into the running MVC controller set. Wraps `ApplicationPartManager` and the `MvcActionDescriptorChangeProvider` so plugin-style modules can be loaded after startup and have their `IDynamicApiController` services exposed without restarting the host.

## Key Files
| File | Description |
|------|-------------|
| `IDynamicApiRuntimeChangeProvider.cs` | Public contract: `AddAssemblies`, `AddAssembliesWithNotifyChanges`, `RemoveAssemblies` (overloads), `NotifyChanges`. |
| `DynamicApiRuntimeChangeProvider.cs` | Internal default implementation. Manipulates `ApplicationPartManager.ApplicationParts` then triggers the MVC change-provider token. |

## For AI Agents

### Working in this directory
- Used by codegen / plugin-loader code to register newly emitted assemblies (e.g. `Poxiao.CodeGen` outputs).
- Always call a `*WithNotifyChanges` variant when the change should be reflected in Swagger / route table immediately; otherwise route discovery will be stale until `NotifyChanges()` is invoked.
- Removal expects either an `Assembly[]` or assembly-name `string[]` — match by `Assembly.GetName().Name`.

### Common patterns
- DI: register `IDynamicApiRuntimeChangeProvider` -> `DynamicApiRuntimeChangeProvider` in `../Extensions/`.

## Dependencies
### Internal
- `../Providers/MvcActionDescriptorChangeProvider`.
### External
- `Microsoft.AspNetCore.Mvc.ApplicationParts`, `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
