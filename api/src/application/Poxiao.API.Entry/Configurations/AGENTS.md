<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Configurations

## Purpose
Split JSON configuration files for the API host. Furion auto-binds each file as a config section using `services.AddConfigurableOptions<T>()` (see `Startup.ConfigureServices`). Sensitive defaults (DB password, RabbitMQ creds, JWT secret) live here and are typically overridden per environment via `.env` / `appsettings.{ENV}.json`.

## Key Files
| File | Description |
|------|-------------|
| `AppSetting.json` | `AppSettings.DefaultPassword`, `InjectMiniProfiler`, plus `Lab.Formula` precision options bound to `Poxiao.Lab.Entity.Config.LabOptions`. |
| `ConnectionStrings.json` | SqlSugar `ConnectionConfigs` array (default + `Poxiao-Job`); MySQL `lumei` DB. |
| `JWT.json` | JWT issuer / audience / secret and lifetime for the `AddJwt<JwtHandler>` setup. |
| `EventBus.json` | EventBus type (`RabbitMQ`) with `HostName`, credentials — consumed by `RabbitMQEventSourceStorer`. |
| `Cache.json` | Cache provider (Memory / Redis) options for `CacheOptions`. |
| `Cors.json` | Origins allow-list for `services.AddCorsAccessor()`. |
| `OSS.json` | Object storage provider config (Aliyun / Minio / Local), bound in `OSSServiceConfigureExtensions`. |
| `AI.json` | Endpoints, model names and API keys for `Poxiao.AI` (vLLM, embeddings, vector store). |
| `AppearanceFeaturePrompt.json` | Prompt templates for `IAppearanceFeatureAnalysisService`. |
| `Logging.json` | Serilog / `AddFileLogging` per-level filters. |
| `Swagger.json` | Knife4jUI groups / route templates. |
| `Tenant.json` | Multi-tenant runtime settings (`TenantOptions`). |
| `App.json` | General app metadata (name, urls, versions). |

## For AI Agents

### Working in this directory
- Never commit real production secrets — values shipped here are dev/test defaults. The `ConnectionStrings.json` currently contains a live-looking MySQL endpoint; replace with placeholders before pushing public.
- Each file's top-level key matches the Furion section name read by `App.GetOptions<T>()` and `App.GetConfig<T>("OSS", true)`.
- `*.json` files are copied to output via `<Content Update="Configurations\AI.json">` (and similar) entries in the csproj — adding a new config file may require an explicit copy rule.
- Lab module configuration uses the `"Lab"` section in `AppSetting.json` (see `Startup.cs` → `services.Configure<LabOptions>(... "Lab" ...)`).

### Common patterns
- One JSON file per options class; flat sections only.
- Connection strings use `{0}` placeholder replaced by `DBName` at runtime in `SqlSugarConfigureExtensions.SetDbConfig`.

## Dependencies
### Internal
- Consumed by `../Startup.cs`, `../Extensions/SqlSugarConfigureExtensions.cs`, `../Extensions/OSSServiceConfigureExtensions.cs`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
