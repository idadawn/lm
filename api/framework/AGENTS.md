<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# framework

## Purpose
Vendored Poxiao framework — an in-house Furion/Sukt-style web framework providing the DI, AOP, configuration, scheduling, JWT, caching, logging, EventBus, validation and DynamicApi infrastructure that all `src/modularity/*` modules build on. Consumed as project references (not NuGet) to allow inline patching.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.sln` | Sub-solution containing only the framework + framework tests; useful for working on framework changes in isolation. |
| `Directory.Build.props` | Framework-wide MSBuild props (analyzer config, lang version). |
| `.editorconfig` | StyleCop/dotnet style rules applied to framework code. |
| `LICENSE` | License for the vendored framework code. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao/` | Core framework package (`net10.0`, `Description=Poxiao 帮助类`) — App, AspNetCore, Authorization, Cache, ConfigurableOptions, DependencyInjection, DynamicApiController, EventBus, FriendlyException, Schedule, SpecificationDocument, UnifyResult, ViewEngine, etc. |
| `Poxiao.Extras.Authentication.JwtBearer/` | JWT bearer auth extension (`AddJwt<THandler>`, `JWTEncryption`). |
| `Poxiao.Extras.DatabaseAccessor.Dapper/` | Dapper repositories + extensions. |
| `Poxiao.Extras.DatabaseAccessor.SqlSugar/` | SqlSugar repositories, `ISqlSugarRepository<T>`, `SqlSugarUnitOfWork`, options. The primary ORM the project uses. |
| `Poxiao.Extras.DependencyModel.CodeAnalysis/` | Roslyn-based dependency-model helpers. |
| `Poxiao.Extras.Logging.Serilog/` | Serilog wiring extensions. |
| `Poxiao.Extras.ObjectMapper.Mapster/` | Mapster registration extensions. |
| `Poxiao.Xunit/` | xUnit harness — `TestStartup`, `AssemblyFixtureAttribute`, custom xUnit extensions used by `tests/`. |

## For AI Agents

### Working in this directory
- This is a vendored framework — prefer extending via the consuming module rather than editing core. If a fix is required here, keep it minimal and document the deviation.
- All entity-related code expected to interop with consumers must respect `CLDEntityBase` field-naming rules (see `/data/project/lm/.cursorrules`).
- The `Poxiao` package embeds resources (`FriendlyException/Assets/error.html`, `Schedule/Dashboard/frontend/**`, `SpecificationDocument/Assets/index*.html`) — when adding files under those paths update `Poxiao.csproj`.
- Targets `net10.0` with `Nullable=enable` and `AllowUnsafeBlocks=true`.

### Common patterns
- `App.GetOptions<TOptions>()` pulls from `services.AddConfigurableOptions<TOptions>()`.
- DI lifetimes via marker interfaces: `ITransient`, `IScoped`, `ISingleton` (auto-registered by `Poxiao.DependencyInjection`).
- Web hosting bootstrapped via `Serve.Run(RunOptions.Default.AddWebComponent<...>())` — see `Poxiao/App/Serve.cs`.

## Dependencies
### Internal
- Consumed by every project under `../src/`.

### External
- `Microsoft.AspNetCore.App` framework reference, `Swashbuckle.AspNetCore` 6.5.0, `CSRedisCore` 3.8.807, `MiniProfiler.AspNetCore.Mvc` 4.5.4 (Poxiao core); SqlSugar, Dapper, MailKit, Mapster, Serilog in extras.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
