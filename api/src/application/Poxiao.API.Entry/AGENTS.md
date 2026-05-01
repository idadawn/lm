<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.API.Entry

## Purpose
Web API host for the Laboratory Data Analysis System. Boots the Poxiao framework (Furion-derived) via `Serve.Run` + `WebComponent`, wires every `Poxiao.*` modular project (lab, ai, system, workflow, codegen, kpi, message, oauth, etc.) into a single ASP.NET Core 10 process, and exposes Knife4j (Swagger) on `/newapi`, JWT auth, RabbitMQ EventBus and a WebSocket endpoint at `/api/message/websocket`.

## Key Files
| File | Description |
|------|-------------|
| `Program.cs` | Entry point — defines `WebComponent`, registers AI services, configures Kestrel (50MB request limit) and console log formatter. |
| `Startup.cs` | Main `AppStartup` — SqlSugar, JWT (`JwtHandler`), CORS, schedule, EventBus (RabbitMQ via `RabbitMQEventSourceStorer`), Senparc Weixin, file logging, Lab `CalcTaskPublisher`/`CalcProgressConsumer` wiring. |
| `ConfigurationHelper.cs` | `.env` loader (`AddDotNetEnv`) and required-value getters keyed off `ASPNETCORE_ENVIRONMENT`. |
| `Poxiao.API.Entry.csproj` | SDK `Microsoft.NET.Sdk.Web`, Knife4jUI, Microsoft.Extensions.AI 9.0.1-preview; `ProjectReference`s to all 14 modularity projects + framework SqlSugar. |
| `Dockerfile` | Multi-stage build on `mcr.microsoft.com/dotnet/sdk:10.0` → `aspnet:10.0`, copies `Directory.Build.props`. |
| `Program.Production.cs` | Production-specific bootstrap variant. |
| `appsettings.json` | Top-level settings; per-section configs live in `Configurations/`. |
| `sensitive-words.txt` | Embedded resource consumed by `services.AddSensitiveDetection()`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Configurations/` | Split JSON config files (AppSetting, ConnectionStrings, JWT, EventBus, OSS, AI, Tenant, ...). See `Configurations/AGENTS.md`. |
| `Extensions/` | `IServiceCollection` extensions for SqlSugar, OSS, Lab DB seeding. See `Extensions/AGENTS.md`. |
| `Handlers/` | Custom `AppAuthorizeHandler` (`JwtHandler`). See `Handlers/AGENTS.md`. |
| `Properties/` | `launchSettings.json` profiles (dev / YS / YY / wdev / Docker). See `Properties/AGENTS.md`. |
| `lib/` | Native interop binaries (yitidgengo for snowflake worker IDs). See `lib/AGENTS.md`. |
| `.config/` | dotnet-ef tool manifest. See `.config/AGENTS.md`. |

## For AI Agents

### Working in this directory
- This is the only ASP.NET host process — `Program.cs` and `Startup.cs` are the place to register cross-cutting middleware, never inside individual modularity projects.
- New module projects must be added both as a `<ProjectReference>` here and (if they expose controllers) discovered automatically via Furion conventions; otherwise services need explicit `services.Add...` calls in `Startup.ConfigureServices`.
- Do not change JWT `NameClaimType = "Account"` or the `OnMessageReceived` token-from-query behaviour without coordinating with the frontend.
- Lab calc pipeline is initialized in `Configure(...)` after `EventBusOptions` is loaded — the corresponding consumer process is `Poxiao.Lab.CalcWorker`.
- DB tables are created lazily via `DatabaseInitExtension` — call is currently commented out; re-enable carefully on a fresh DB.

### Common patterns
- `App.GetOptions<TOptions>()` to read JSON-bound settings (Furion).
- `services.AddConfigurableOptions<T>()` for hot-reloadable options.
- `RESTfulResultProvider` + `RequestActionFilter` shape every controller response.
- Logs split per `LogLevel` into rolling daily files via `AddFileLogging`.

## Dependencies
### Internal
- `framework/Poxiao.Extras.DatabaseAccessor.SqlSugar`
- All 14 `src/modularity/*` projects (ai, app, codegen, extend, kpi, lab, message, oauth, subdev, system, taskschedule, visualdata, visualdev, workflow)

### External
- `IGeekFan.AspNetCore.Knife4jUI` 0.0.13, `Microsoft.Extensions.AI.Abstractions` 9.0.1-preview, `Microsoft.Extensions.Hosting.WindowsServices` / `.Systemd`, `RabbitMQ.Client`, `Senparc.Weixin`, `OnceMi.AspNetCore.OSS`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
