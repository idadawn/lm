<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Defines the `IConfigurableOptions` interface family — the marker + opt-in capability contracts an options POCO uses to plug into the ConfigurableOptions binding pipeline (post-configure, complex validation, live-reload listener).

## Key Files
| File | Description |
|------|-------------|
| `IConfigurableOptions.cs` | Four partial interfaces: marker `IConfigurableOptions`; `IConfigurableOptions<TOptions>` adding `PostConfigure(options, configuration)`; `IConfigurableOptions<TOptions, TOptionsValidation>` registering a `class, IValidateOptions<TOptions>` for complex validation; `IConfigurableOptionsListener<TOptions>` adding `OnListener(options, configuration)` invoked on `ChangeToken` reloads. |

## For AI Agents

### Working in this directory
- All four interfaces are declared `partial` so a feature module can extend them in its own assembly without forking — preserve that.
- Don't add concrete implementations here — concrete options (e.g. `CorsAccessorSettingsOptions`) live next to the feature they configure.
- The validator generic constraint `where TOptionsValidation : class, IValidateOptions<TOptions>` is enforced by `ConfigurableOptionsServiceCollectionExtensions` at runtime via `TryAddEnumerable`.

### Common patterns
- Layered marker + capability interfaces (post-configure, validate, listen) — opt in by inheritance.
- Only the base marker is required; the others are detected by reflection (`GetInterfaces().FirstOrDefault(... GenericTypeDefinition ...)` and `IsAssignableFrom`).

## Dependencies
### Internal
- Consumed by `ConfigurableOptions/Extensions/ConfigurableOptionsServiceCollectionExtensions.cs`.
### External
- `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Options` (for `IValidateOptions<>`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
