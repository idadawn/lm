<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Implementation helper that translates `CorsAccessorSettingsOptions` into a `CorsPolicyBuilder` configuration, including all the SignalR-compat workarounds (forced GET/POST, no `AllowAnyOrigin` when credentials are required) and the LIMS-specific exposed-headers defaults (`access-token`, `x-access-token`).

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | `internal static` helper. `SetCorsPolicy(builder, settings, isMiddleware=false)` calls `SetIsOriginAllowed(_ => true)` then conditionally applies `WithOrigins` (with wildcard subdomains), `WithHeaders` / `AllowAnyHeader`, `WithMethods` / `AllowAnyMethod` (force-merging GET/POST for SignalR), `AllowCredentials`, `WithExposedHeaders` (default tokens injected when `FixedClientToken=true`), and `SetPreflightMaxAge` (defaulting 24h). |

## For AI Agents

### Working in this directory
- Keep the `_defaultExposedHeaders = ["access-token", "x-access-token"]` array in sync with what the Vue frontend (`web/src/utils/http`) reads — the Chinese-language code base relies on this contract for JWT delivery.
- `SetIsOriginAllowed(_ => true)` is intentional and combined with `WithOrigins(...).SetIsOriginAllowedToAllowWildcardSubdomains()` so subdomain wildcards work; do not remove the always-true predicate without re-validating with the LIMS frontend.
- The SignalR branch is explicit and important — `AllowAnyOrigin` cannot be combined with `AllowCredentials`.

### Common patterns
- Defensive null/empty checks on every `WithXxx[]`, falling back to permissive `AllowAny...` when unset.

## Dependencies
### Internal
- `CorsAccessor/Options/CorsAccessorSettingsOptions.cs`.
### External
- `Microsoft.AspNetCore.Cors.Infrastructure`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
