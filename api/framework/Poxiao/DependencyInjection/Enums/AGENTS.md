<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
Enumerations that parameterise Poxiao's DI registration. `InjectionActions` decides whether an existing registration is overwritten or skipped; `InjectionPatterns` decides which interface shape(s) the scanner registers; `RegisterType` maps to `ServiceLifetime` for `appsettings.json`-driven external definitions.

## Key Files
| File | Description |
|------|-------------|
| `InjectionActions.cs` | `Add` (overwrite) vs `TryAdd` (skip if present). |
| `InjectionPatterns.cs` | `Self`, `FirstInterface`, `SelfWithFirstInterface` (default), `ImplementedInterfaces`, `All`. |
| `RegisterType.cs` | `Transient` / `Scoped` / `Singleton` for external service definitions. |

## For AI Agents

### Working in this directory
- Each enum value has a `[Description]` attribute used in admin UIs and diagnostics — keep the Chinese descriptions accurate when adding values.
- Adding a value usually requires updating the matching switch in `../Extensions/DependencyInjectionServiceCollectionExtensions.cs` and `../Internal/ExternalService.cs`.

### Common patterns
- Enums are decorated with `[SuppressSniffer]`.
- `InjectionPatterns.All` is the most permissive and is the implicit default for `[Injection]`.

## Dependencies
### Internal
- Used by `../Attributes/InjectionAttribute.cs` and `../Internal/ExternalService.cs`.
### External
- `System.ComponentModel` (for `DescriptionAttribute`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
