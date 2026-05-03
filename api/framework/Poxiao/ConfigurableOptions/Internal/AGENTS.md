<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Implementation helpers for the ConfigurableOptions feature. Resolves the appsetting.json section path for a given options type and exposes a fallback path for retrieving options *before* the host has built `App.RootServices` (used during early bootstrap by features such as `CorsAccessor`).

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | `internal static` helper. `GetOptionsConfiguration(Type)` returns `(OptionsSettingsAttribute, path)`: when `[OptionsSettings(path)]` is present it wins, otherwise the class name with a trailing `Options` suffix is stripped (e.g. `CorsAccessorSettingsOptions` → `CorsAccessorSettings`). `GetOptionsOnStarting<TOptions>()` returns options via `App.GetConfig<TOptions>(path, true)` while DI is not yet ready (a v4.5.2+ upgrade-compat fallback). |

## For AI Agents

### Working in this directory
- Keep both methods `internal` and the class named `Penetrates` — convention is repo-wide.
- The naming convention (drop trailing `Options`) is part of the public contract; users rely on it. Do not change without coordinating with every existing options class.
- `GetOptionsOnStarting` returns `null` once `App.RootServices` is set — callers must fall back to `IOptions<TOptions>` after host build.

### Common patterns
- Switch expression keyed on `OptionsSettingsAttribute` null/empty Path branches.
- Two-phase bootstrap: pre-DI snapshot via `App.GetConfig`, post-DI live binding via `IOptions<>`.

## Dependencies
### Internal
- `ConfigurableOptions/Attributes/OptionsSettingsAttribute.cs`, `ConfigurableOptions/Options/IConfigurableOptions.cs`, `App`.
### External
- `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
