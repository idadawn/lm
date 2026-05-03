<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Properties

## Purpose
ASP.NET Core launch profiles for the API host. Profiles map to `ASPNETCORE_ENVIRONMENT` values and pin development URLs so the frontend dev proxy (`web/.env.development`) and the QA / production-staging environments stay in sync.

## Key Files
| File | Description |
|------|-------------|
| `launchSettings.json` | Five profiles — `dev` (port 9530, no browser), `wdev` (port 9002), `YS` (port 9530), `YY` (port 10089), `Docker`. The `dev` profile is what `dotnet watch run --launch-profile dev` (per project `CLAUDE.md`) uses. |

## For AI Agents

### Working in this directory
- Frontend `pnpm dev` proxies to `http://localhost:9530` — do not change the `dev` profile port without updating `web/.env.development` and the mock server.
- `YS` and `YY` are real customer environment names (not generic "staging"); coordinate before renaming.
- `launchSettings.json` is developer-machine-only and not deployed; production hosting (Docker/systemd) reads `ASPNETCORE_ENVIRONMENT` from the runtime environment instead.

### Common patterns
- `commandName: Project` for `dotnet run` profiles, `Docker` for the IDE container target.
- `applicationUrl: http://*:<port>` to bind on all interfaces in dev.

## Dependencies
### Internal
- Consumed indirectly by `ConfigurationHelper.AddEnvironmentVariables` which selects `.env.{ENV}` based on `ASPNETCORE_ENVIRONMENT`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
