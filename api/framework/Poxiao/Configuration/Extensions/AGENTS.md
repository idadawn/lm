<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Public API surface for the Configuration feature. Adds Poxiao's `AddFile(...)` to `IConfigurationBuilder` (path-prefix aware, env-overlay, hot-reload, json/xml/ini) and typed `Get<T>` / `Exists` helpers on `IConfiguration`. Driver for the bootstrap pipeline that loads `AppSetting.json`, `ConnectionStrings.json`, `Cache.json` and friends.

## Key Files
| File | Description |
|------|-------------|
| `IConfigurationBuilderExtensions.cs` | `AddFile(builder, fileName, env, optional, reloadOnChange, includeEnvironment)`. Validates the file-name pattern, resolves an absolute path via prefix grammar (`&` / `.` AppContext.BaseDirectory, `/` / `!` literal, `@` / `~` cwd), creates a typed `JsonConfigurationSource` / `XmlConfigurationSource` / `IniConfigurationSource`, then optionally registers an `AppSetting.{env}.json` overlay. |
| `IConfigurationExtensions.cs` | Convenience overloads on `IConfiguration`: `Exists(key)`, `Get<T>(key)`, `Get<T>(key, configureOptions)`, `Get(key, type)`, `Get(key, type, configureOptions)` — each shorthand for `GetSection(key).Get<T>(...)`. |

## For AI Agents

### Working in this directory
- Keep both classes `[SuppressSniffer]` and in their respective MS namespaces (`Microsoft.Extensions.Configuration`) so they appear next to BCL extensions in IntelliSense.
- Argument order on `AddFile` is part of the public contract — append parameters at the tail with defaults.
- The path-prefix grammar (`& . / ! @ ~`) is documented behaviour; if you add a new prefix, document it in the XML doc comment and update the constants regex if it affects file-name parsing.

### Common patterns
- Inline parameter parsing pulls boolean overrides out of the file-name itself, then falls back to method arguments.
- File source created via switch-expression on extension; `ResolveFileProvider()` is called to populate `FileProvider`.

## Dependencies
### Internal
- `Configuration/Constants/Constants.cs`.
### External
- `Microsoft.Extensions.Configuration.{Json,Xml,Ini}`, `Microsoft.Extensions.Hosting`, `System.Text.RegularExpressions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
