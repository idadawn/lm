<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Strongly-typed options bound from `appsettings.json -> DynamicApiControllerSettings`. Drives global naming/routing decisions (default route prefix, default HTTP method, casing rules, suffix stripping) consumed by `../Conventions/DynamicApiControllerApplicationModelConvention`.

## Key Files
| File | Description |
|------|-------------|
| `DynamicApiControllerSettingsOptions.cs` | `IConfigurableOptions<TSelf>` carrying `DefaultRoutePrefix`, `DefaultHttpMethod` (required), `DefaultModule`, `LowercaseRoute`, `AsLowerCamelCase`, `KeepVerb`, `KeepName`, plus suffix-stripping arrays. |

## For AI Agents

### Working in this directory
- The class name = configuration section name. Renaming requires updating every `appsettings*.json` across the API entry point.
- `DefaultHttpMethod` is `[Required]`; default is `"POST"` per AppSetting.json conventions. Validate before changing.
- Avoid putting business-domain logic here — this is purely framework configuration.

### Common patterns
- Nullable booleans (`bool?`) so absence in JSON keeps framework-side defaults.

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions`.
### External
- `Microsoft.Extensions.Configuration`, `System.ComponentModel.DataAnnotations`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
