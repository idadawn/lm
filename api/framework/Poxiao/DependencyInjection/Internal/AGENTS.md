<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Internal model used to describe a service registration that originates from configuration (e.g. `appsettings.json` -> `DependencyInjectionSettings.Definitions`) rather than a `[Injection]`-decorated class. Lets ops bind interface ↔ implementation pairs in `assembly;FullName` string form, including optional `Proxy` interception, without recompiling.

## Key Files
| File | Description |
|------|-------------|
| `ExternalService.cs` | DTO with `Interface`, `Service`, `RegisterType`, `Action`, `Pattern`, `Named`, `Order`, `Proxy` properties consumed by `../Extensions/DependencyInjectionServiceCollectionExtensions.cs`. |

## For AI Agents

### Working in this directory
- Property names map 1:1 to keys in JSON configuration — renaming is a breaking change for ops.
- `Interface` / `Service` / `Proxy` use the `"AssemblyName;FullTypeName"` format; the resolver in `Extensions/` parses this with `Type.GetType(...)`.

### Common patterns
- Pairs with `../Options/DependencyInjectionSettingsOptions.cs` (the bound options class).
- Defaults: `Action = Add`, `Pattern = All`.

## Dependencies
### Internal
- `../Enums/RegisterType`, `../Enums/InjectionActions`, `../Enums/InjectionPatterns`.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
