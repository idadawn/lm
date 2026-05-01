<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
JSON-bound options for the DataValidation feature. Lets `AppSetting.json` carry an array of `(type, key, message)` triples that override the default error messages baked into `ValidationTypes`. Bound by `AddDataValidation` via `services.AddConfigurableOptions<ValidationTypeMessageSettingsOptions>()`.

## Key Files
| File | Description |
|------|-------------|
| `ValidationTypeMessageSettingsOptions.cs` | Sealed `IConfigurableOptions`. Single property `object[][] Definitions` — each inner array is `[validationType, key, message]` and is fed to `DataValidator` so users can re-message any validation entry from configuration. |

## For AI Agents

### Working in this directory
- Note this is a different "Options" folder from `DataValidation/Extensions/Options/` — that one holds the DI configurator, this one holds the JSON-bound settings.
- The section name follows the standard convention (drop trailing `Options`): JSON key is `ValidationTypeMessageSettings`.
- `object[][]` is intentional so JSON authors can mix enum-name strings, regex strings, and message strings without a typed schema; if you tighten this, also update the consumers.

### Common patterns
- `IConfigurableOptions` marker without `<TOptions>` (no PostConfigure / validation needed).

## Dependencies
### Internal
- `ConfigurableOptions/Options/IConfigurableOptions.cs`, consumed by `DataValidation/Validators/DataValidator.cs`.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
