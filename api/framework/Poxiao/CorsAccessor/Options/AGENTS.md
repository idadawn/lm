<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Typed configuration POCO for the CorsAccessor feature. Maps the `CorsAccessorSettings` section of `AppSetting.json` and supplies sensible defaults via `IConfigurableOptions<TOptions>.PostConfigure`.

## Key Files
| File | Description |
|------|-------------|
| `CorsAccessorSettingsOptions.cs` | Sealed `IConfigurableOptions<CorsAccessorSettingsOptions>`. `[Required]` `PolicyName` (defaults `"App.Cors.Policy"`), `WithOrigins[]` / `WithHeaders[]` / `WithExposedHeaders[]` / `WithMethods[]` (any null/empty = allow-any), nullable `AllowCredentials` (default `true`), `SetPreflightMaxAge` seconds (defaults 24h via `Penetrates`), `FixedClientToken` (default `true` — exposes `access-token` / `x-access-token`), `SignalRSupport` (default `false`). |

## For AI Agents

### Working in this directory
- `[Required] PolicyName` is enforced via `ValidateDataAnnotations`; `PostConfigure` then assigns a default — keep PostConfigure assigning *after* the Required attribute is satisfied (the required check sees the bound JSON, not the default).
- This class is the single source of truth for the section name; together with `OptionsSettings` convention (drop trailing `Options`) the JSON key is `CorsAccessorSettings`.
- New booleans should stay nullable so `PostConfigure` can apply defaults; raw `bool` would prevent distinguishing "unset" from "false".

### Common patterns
- Nullable primitives with PostConfigure defaulting — repo-wide convention for Poxiao options.

## Dependencies
### Internal
- `ConfigurableOptions/Options/IConfigurableOptions.cs`.
### External
- `System.ComponentModel.DataAnnotations`, `Microsoft.Extensions.Configuration`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
