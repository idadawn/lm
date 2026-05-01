<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extras.Authentication.JwtBearer

## Purpose
Pluggable JWT bearer authentication adapter for the Poxiao framework. Wraps `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0.1) and exposes high-level helpers for issuing, refreshing, validating, and decoding tokens with configurable algorithms, signing keys, and clock-skew tolerance.

## Key Files
| File | Description |
|------|-------------|
| `JWTEncryption.cs` | Static `JWTEncryption` API in `Poxiao.DataEncryption` — token issue/decode/refresh, payload combine, validation-parameter creation, framework-context resolution. |
| `Poxiao.Extras.Authentication.JwtBearer.csproj` | net10.0 packable library targeting AspNetCore JwtBearer 10.0.1. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `AddJwt` DI extensions for `AuthenticationBuilder` / `IServiceCollection` (see `Extensions/AGENTS.md`). |
| `Options/` | `JWTSettingsOptions` POCO bound to `JWTSettings` config section (see `Options/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Token contracts are exposed via static `JWTEncryption` — keep new helpers static and side-effect free; runtime config is fetched through `App.GetOptions<JWTSettingsOptions>` reflectively.
- Prefer `JsonWebTokenHandler` (already used) over the legacy `JwtSecurityTokenHandler` for new flows.
- Never hard-code keys or algorithms — read from `JWTSettingsOptions` so tenants can override `IssuerSigningKey`, `Algorithm`, `ExpiredTime`, `ClockSkew`.

### Common patterns
- Reflective access to the host framework (`JWTEncryption.FrameworkApp`) — keeps this extras assembly decoupled from `Poxiao.csproj`.
- `UnsafeRelaxedJsonEscaping` for payload serialization to keep CJK claims readable.
- Refresh-token flow caches token state in `IDistributedCache`; respect `ClockSkew` semantics.

## Dependencies
### Internal
- Resolves `Poxiao.App` and `Poxiao.Authorization.JWTSettingsOptions` via reflection at runtime (no compile-time project reference).
### External
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.1, `Microsoft.IdentityModel.JsonWebTokens`, `System.IdentityModel.Tokens.Jwt`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
