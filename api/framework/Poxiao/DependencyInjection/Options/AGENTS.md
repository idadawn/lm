<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Strongly-typed options bound from `appsettings.json` (`DependencyInjectionSettings` section) via Poxiao's `IConfigurableOptions`. Carries the array of `ExternalService` definitions that the scanner registers in addition to attribute-driven discovery.

## Key Files
| File | Description |
|------|-------------|
| `DependencyInjectionSettingsOptions.cs` | `Definitions: ExternalService[]`, with `PostConfigure` defaulting to an empty array. |

## For AI Agents

### Working in this directory
- The class name (`DependencyInjectionSettingsOptions`) is the implicit configuration section key — do not rename without updating every `appsettings*.json`.
- Use `IConfigurableOptions<TSelf>` (Poxiao convention), not raw `IOptions<T>` — this enables auto-binding by `AddConfigurableOptions`.

### Common patterns
- `PostConfigure` always normalises null collections to empty arrays so downstream `foreach` is safe.
- Class is `sealed` — extend via composition, not inheritance.

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions`, `../Internal/ExternalService`.
### External
- `Microsoft.Extensions.Configuration`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
