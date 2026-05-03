<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
DI registration helpers that wire JWT bearer authentication into ASP.NET Core. Provides the `AddJwt` overloads used by `Poxiao.API.Entry` to register the auth scheme, validation parameters, and optional global `[Authorize]` filter.

## Key Files
| File | Description |
|------|-------------|
| `JWTAuthorizationServiceCollectionExtensions.cs` | `AddJwt` extension on `AuthenticationBuilder` and `IServiceCollection`; reads `JWTSettingsOptions`, builds `TokenValidationParameters`, and optionally adds a global `AuthorizeFilter`. |

## For AI Agents

### Working in this directory
- Extension methods live in the `Microsoft.Extensions.DependencyInjection` namespace by convention — preserve that so `using` statements stay short at call sites.
- Two overloads exist (one on `AuthenticationBuilder`, one on `IServiceCollection`); keep their behavior in sync when adding parameters.
- `enableGlobalAuthorize` toggles a global `AuthorizeFilter`; document the flag in the API entry-point startup if you change its default.

### Common patterns
- `Assembly.GetCallingAssembly()` is passed to `JWTEncryption.GetFrameworkContext` so the extras can locate the host `Poxiao` framework reflectively.
- `tokenValidationParameters` is typed as `object` to avoid forcing a `Microsoft.IdentityModel.Tokens` reference on consumers.

## Dependencies
### Internal
- Calls into `JWTEncryption` and `JWTSettingsOptions` from the parent project.
### External
- `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.AspNetCore.Mvc.Authorization`, `Microsoft.IdentityModel.Tokens`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
