<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
Declarative metadata for the ConfigurableOptions feature. Lets an options POCO override the default section-name convention (`[OptionsSettings(path)]`) and remap individual properties to a differently-named JSON key (`[MapSettings(path)]`). The extensions in `ConfigurableOptions/Extensions` reflect on these to drive binding.

## Key Files
| File | Description |
|------|-------------|
| `OptionsSettingsAttribute.cs` | Class-level attribute. `Path` overrides the inferred config section; `PostConfigureAll` switches between `optionsConfigure.PostConfigure` and `services.PostConfigureAll<TOptions>` paths. Sealed, single-use. |
| `MapSettingsAttribute.cs` | Property/Field-level attribute carrying a `Path` string. Lets a property re-target a different JSON key under the same section, applied via `services.PostConfigureAll<TOptions>` after binding. |

## For AI Agents

### Working in this directory
- Both attributes are `[SuppressSniffer]` and `sealed`; preserve those modifiers.
- `OptionsSettingsAttribute` is `AttributeUsage(AttributeTargets.Class)` — do not broaden the targets.
- `MapSettingsAttribute` is `AllowMultiple = false` and supports `Property | Field`; `ConfigurableOptionsServiceCollectionExtensions` only iterates *public* properties via `BindingFlags.Instance | BindingFlags.Public`, so don't promise field support unless you also extend the reflection scan.

### Common patterns
- Multiple constructors capturing different attribute parameter combinations (path-only, postConfigureAll-only, both).

## Dependencies
### Internal
- Read by `ConfigurableOptions/Extensions/ConfigurableOptionsServiceCollectionExtensions.cs` and `ConfigurableOptions/Internal/Penetrates.cs`.
### External
- `System` (Attribute base only).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
