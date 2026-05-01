<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Strongly-typed JWT settings bound from the host application's `appsettings.json` (`JWTSettings` section). Drives token issuance and validation in `JWTEncryption` and the `AddJwt` extensions.

## Key Files
| File | Description |
|------|-------------|
| `JWTSettingsOptions.cs` | Sealed POCO holding `IssuerSigningKey`, `ValidIssuer`/`ValidAudience`, validation toggles, `ClockSkew` (seconds), `ExpiredTime` (minutes), and `Algorithm` (e.g., `HmacSha256`). |

## For AI Agents

### Working in this directory
- All numeric/boolean settings are nullable on purpose — `null` means "use framework default" inside `JWTEncryption`.
- Property names must stay aligned with `JWTSettings:*` keys in `Configurations/AppSetting.json` of the API entry project; renaming is a breaking config change.
- Keep this class POCO-only (no logic, no attributes beyond config binding) — it is fetched reflectively via `App.GetOptions<JWTSettingsOptions>`.

### Common patterns
- Lives in `Poxiao.Authorization` namespace (not the extras namespace) so the host framework can reference it without a project dependency.

## Dependencies
### External
- None beyond the BCL.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
