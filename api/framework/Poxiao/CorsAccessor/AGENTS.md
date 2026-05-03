<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CorsAccessor

## Purpose
Pluggable CORS feature for Poxiao. A single `AddCorsAccessor` / `UseCorsAccessor` pair binds a typed `CorsAccessorSettingsOptions` from `appsetting.json` (`CorsAccessorSettings` section) and applies origin / header / method / credentials / preflight policy with optional SignalR-aware behaviour and "fixed client token" header exposure (`access-token`, `x-access-token`).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `AddCorsAccessor` / `UseCorsAccessor` public extensions (see `Extensions/AGENTS.md`). |
| `Internal/` | `Penetrates.SetCorsPolicy` policy builder helper (see `Internal/AGENTS.md`). |
| `Options/` | `CorsAccessorSettingsOptions` POCO (see `Options/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Configuration is read both eagerly (during `AddCorsAccessor` via `App.GetConfig<...>("CorsAccessorSettings", true)`) *and* through `IOptions<>` at middleware time — keep both code paths consistent if you change the section name.
- SignalR mode requires `AllowCredentials` + concrete origins; the helper handles forcing GET/POST and exposes that branch in `Penetrates`. Don't simplify into `AllowAnyOrigin` blindly.
- `FixedClientToken` is the LIMS-specific switch that exposes JWT headers to the browser — keep it on by default to avoid breaking the Vue web client.

### Common patterns
- Single `IConfigurableOptions<TOptions>` implementation with `PostConfigure` filling in defaults (`PolicyName = "App.Cors.Policy"`, `AllowCredentials = true`, etc.).

## Dependencies
### Internal
- `ConfigurableOptions/`, `App` (eager config access).
### External
- `Microsoft.AspNetCore.Cors`, `Microsoft.Extensions.DependencyInjection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
