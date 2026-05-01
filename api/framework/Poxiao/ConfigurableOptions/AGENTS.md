<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ConfigurableOptions

## Purpose
Poxiao's typed-options feature: lets a settings POCO declare itself as `IConfigurableOptions`, optionally tag itself with `[OptionsSettings]`, and be auto-bound to an `appsetting.json` section with validation, post-configure callbacks, change-token listeners and per-property remap support. Used by virtually every other Poxiao feature (`CorsAccessor`, `DataEncryption`, `DataValidation`, etc.) to expose typed configuration.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[OptionsSettings]` and `[MapSettings]` attributes (see `Attributes/AGENTS.md`). |
| `Extensions/` | `AddConfigurableOptions<TOptions>` service-collection extension (see `Extensions/AGENTS.md`). |
| `Internal/` | `Penetrates` helper resolving the configuration path and bootstrap-time options (see `Internal/AGENTS.md`). |
| `Options/` | The `IConfigurableOptions` interface family (see `Options/AGENTS.md`). |

## For AI Agents

### Working in this directory
- New options classes go *outside* this folder (each feature owns its own Options); only generic infrastructure lives here.
- Honor the section-name convention: class name, drop trailing `Options`, or `[OptionsSettings(path)]` overrides it (see `Internal/Penetrates.GetOptionsConfiguration`).
- Bound `BindNonPublicProperties = true` is intentional — many Furion-derived options use init-only or private setters.

### Common patterns
- Marker interface (`IConfigurableOptions`) + generic variants (`<TOptions>`, `<TOptions, TValidation>`, `IConfigurableOptionsListener<TOptions>`) for opt-in features.
- Post-configure runs validation logic; `ChangeToken.OnChange` rebinds on file edit.

## Dependencies
### Internal
- `Configuration/` (path resolution), `App` (root services / configuration).
### External
- `Microsoft.Extensions.Options`, `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Primitives`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
