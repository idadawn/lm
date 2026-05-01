<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Constants

## Purpose
Internal constants for the Options module. Currently holds the convention-over-configuration suffix used to derive a configuration section key from an options class name.

## Key Files
| File | Description |
|------|-------------|
| `Constants.cs` | `internal const string OptionsTypeSuffix = "Options"` — stripped from the class name (via `ClearStringAffixes`) when an `[OptionsBuilder]` attribute is missing or has no explicit `SectionKey`. |

## For AI Agents

### Working in this directory
- This file is `internal static class Constants` — keep it internal; section-key behaviour is part of the framework contract and changing the suffix would silently re-bind every options POCO in the solution.

## Dependencies
### Internal
- Used by `OptionsBuilderExtensions.ConfigureDefaults` to compute the default `IConfigurationSection` for `optionsBuilder.Bind`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
