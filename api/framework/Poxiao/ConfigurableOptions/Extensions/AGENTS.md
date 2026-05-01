<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Public entry point for the ConfigurableOptions feature. Provides `AddConfigurableOptions<TOptions>()` which binds an `IConfigurableOptions` POCO to its `appsetting.json` section, wires DataAnnotations validation, optional `IValidateOptions<>` enforcement, listener-style change tokens, and `[MapSettings]` per-property remapping.

## Key Files
| File | Description |
|------|-------------|
| `ConfigurableOptionsServiceCollectionExtensions.cs` | Single static class with `AddConfigurableOptions<TOptions>`. Resolves the section path via `Penetrates.GetOptionsConfiguration`, calls `services.AddOptions<TOptions>().Bind(...).ValidateDataAnnotations()`, conditionally registers an `IValidateOptions<TOptions>`, runs `PostConfigure` (per-binding or all instances depending on `[OptionsSettings(PostConfigureAll = true)]`), and subscribes `IConfigurableOptionsListener<TOptions>.OnListener` to `ChangeToken.OnChange` for live reload. |

## For AI Agents

### Working in this directory
- `BindNonPublicProperties = true` is intentional — Poxiao options often use private setters; keep it.
- The `ChangeToken.OnChange` listener has a known double-fire issue in ASP.NET Core (commented in source); add a debounce locally if you wire side effects, don't reformulate the global subscription.
- The `PostConfigureAll` branch differs subtly: `optionsSettings?.PostConfigureAll != true` chooses per-options post-configure; flipping that affects every options class — coordinate with the attribute defaults.

### Common patterns
- `services.AddOptions<>().Bind().ValidateDataAnnotations()` chain.
- Generic-argument introspection to find `IConfigurableOptions<TOptions, TValidation>` and pull the validator type.

## Dependencies
### Internal
- `ConfigurableOptions/Internal/Penetrates.cs`, `ConfigurableOptions/Options/IConfigurableOptions.cs`, `ConfigurableOptions/Attributes/`, `App` (root configuration).
### External
- `Microsoft.Extensions.Options`, `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Primitives`, `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
