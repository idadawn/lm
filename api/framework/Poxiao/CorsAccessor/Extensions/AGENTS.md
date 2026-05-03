<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Public DI / middleware entry points for CorsAccessor. `AddCorsAccessor` registers the typed `CorsAccessorSettingsOptions`, builds the named CORS policy (`PolicyName`) and lets callers tweak `CorsOptions` / `CorsPolicyBuilder`. `UseCorsAccessor` mounts the middleware, either by named policy or — when `SignalRSupport=true` — by inline policy so credentials and concrete origins are applied correctly.

## Key Files
| File | Description |
|------|-------------|
| `CorsAccessorServiceCollectionExtensions.cs` | `AddCorsAccessor(services, corsOptionsHandler?, corsPolicyBuilderHandler?)`. Eagerly fetches `CorsAccessorSettings` via `App.GetConfig`, calls `services.AddCors`, registers the named policy through `Penetrates.SetCorsPolicy`, and forwards optional caller overrides. |
| `CorsAccessorApplicationBuilderExtensions.cs` | `UseCorsAccessor(app, corsPolicyBuilderHandler?)`. Resolves `IOptions<CorsAccessorSettingsOptions>`, branches on `SignalRSupport`: false → `app.UseCors(policyName)`, true → inline `app.UseCors(builder => Penetrates.SetCorsPolicy(builder, settings, true))`. |

## For AI Agents

### Working in this directory
- Both extensions are `[SuppressSniffer]` static — preserve.
- The eager `App.GetConfig` call in `AddCorsAccessor` runs before DI is built; if you replace it with an `IConfiguration` injection you must change `CorsAccessorSettingsOptions.PostConfigure` defaults to ensure they apply pre-DI too.
- The `isMiddleware` flag passed to `Penetrates.SetCorsPolicy` differs between the two extensions — keep them consistent (false from services, true from app builder).

### Common patterns
- Optional caller-supplied `Action<...>` for last-mile customisation after the framework's policy is applied.

## Dependencies
### Internal
- `CorsAccessor/Internal/Penetrates.cs`, `CorsAccessor/Options/CorsAccessorSettingsOptions.cs`, `App`.
### External
- `Microsoft.AspNetCore.Cors.Infrastructure`, `Microsoft.Extensions.DependencyInjection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
