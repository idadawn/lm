<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Constants

## Purpose
Regex pattern strings used by the Configuration feature to parse config file names and inline parameter suffixes. Centralised so `IConfigurationBuilderExtensions.AddFile` and any future tooling stay in sync.

## Key Files
| File | Description |
|------|-------------|
| `Constants.cs` | `internal static class Constants` with nested `Patterns`. `ConfigurationFileName` matches `<realName>.<environmentName>?.<json|xml|ini>` with named capture groups; `ConfigurationFileParameter` matches `\s+name\s*=\s*(true|false)` for inline boolean parameters. |

## For AI Agents

### Working in this directory
- Both constants are `internal const string` — keep them const so they can be inlined into `Regex.IsMatch` / `Regex.Match` call sites.
- Named capture groups (`fileName`, `realName`, `environmentName`, `extension`, `parameter`, `value`) are referenced by name in `IConfigurationBuilderExtensions`; rename only with a coordinated change.
- Adding a new file extension (e.g. `.yaml`) requires updating `ConfigurationFileName` *and* `CreateFileConfigurationSource`.

### Common patterns
- Nested static `Patterns` class for grouping regexes — mirrors Furion's convention.

## Dependencies
### Internal
- Used exclusively by `Configuration/Extensions/IConfigurationBuilderExtensions.cs`.
### External
- None directly (consumed via `System.Text.RegularExpressions`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
