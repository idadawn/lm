<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Configuration

## Purpose
Low-level configuration plumbing for Poxiao: extensions on top of `Microsoft.Extensions.Configuration` that add a path-aware `AddFile(...)` (json/xml/ini, env-aware, hot-reload, `&` / `@` / `~` / `!` / `/` path prefixes) plus typed `Get<T>` helpers on `IConfiguration`. Used during host bootstrap to load `AppSetting.json`, `ConnectionStrings.json`, `Cache.json` etc.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Constants/` | Regex constants for parsing config file names (see `Constants/AGENTS.md`). |
| `Extensions/` | Public `IConfigurationBuilder` / `IConfiguration` extension methods (see `Extensions/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Path-prefix grammar (`&`, `.`, `/`, `!`, `@`, `~`) is part of the public contract — see `IConfigurationBuilderExtensions.ResolveRealAbsolutePath`. Don't change semantics without auditing every `AddFile` call across `api/`.
- Only `.json`, `.xml`, `.ini` are accepted as config sources; adding a new format means extending the switch in `CreateFileConfigurationSource` *and* the `ConfigurationFileName` regex in `Constants`.
- Inline parameter syntax (`AppSetting.json optional=true reloadOnChange=true`) is parsed by `ConfigurationFileParameter` regex; keep `optional`/`reloadOnChange`/`includeEnvironment` names stable.

### Common patterns
- Regex-driven file-name parsing with named capture groups (`fileName`, `realName`, `environmentName`, `extension`).
- Environment overlay: registers both `AppSetting.json` and `AppSetting.{env}.json` when `includeEnvironment=true`.

## Dependencies
### External
- `Microsoft.Extensions.Configuration.{Json,Xml,Ini}`, `Microsoft.Extensions.Hosting`, `System.Text.RegularExpressions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
