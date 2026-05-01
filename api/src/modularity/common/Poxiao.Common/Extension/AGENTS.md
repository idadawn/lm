<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extension

## Purpose
Cross-cutting C# extension methods for the entire backend — strings, dates, enums, dictionaries, enumerables, streams, randoms and booleans. The largest helpers (`StringExtensions.cs` ~34 KB, `RandomExtensions.cs` ~39 KB, `Extensions.DateTime.cs` ~24 KB) are the primary toolbox the rest of the codebase uses for parsing, formatting and Chinese-locale-aware operations.

## Key Files
| File | Description |
|------|-------------|
| `StringExtensions.cs` | String parsing/formatting helpers (numeric, JSON, encoding, masking). |
| `Extensions.cs` | General-purpose object extensions. |
| `Extensions.DateTime.cs` | DateTime helpers (Chinese date formatting, week/quarter/year math). |
| `EnumExtensions.cs` | `Enum`→Dictionary cache, description lookup, name<->value resolution; backs frontend dropdown sources. |
| `RandomExtensions.cs` | Random data/Mock helpers (names, addresses) for fixtures and demos. |
| `EnumerableExtensions.cs` | LINQ-style helpers (paging, distinct-by). |
| `DictionaryExtensions.cs` / `ConcurrentDictionaryExtensions.cs` | Dict get-or-add, merge, AddOrUpdate. |
| `StringBuilderExtensions.cs` | Append helpers. |
| `BooleanExtensions.cs` | `TrueThrow`、`ToLower`、loose-text→bool (`是/否/yes/no/0/1`). |
| `StreamExtensions.cs` | Stream utilities. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Extension`.
- All classes are `static` and tagged `[SuppressSniffer]` — keep both.
- `EnumExtensions` uses two `ConcurrentDictionary` caches keyed by `Type`; never invalidate them at runtime.
- Place new domain-specific extensions in the matching feature module, not here — this folder is reserved for truly generic primitives.
- When adding to `StringExtensions`/`Extensions.DateTime`, group thematically; partial-class style splits (`Extensions.cs` + `Extensions.DateTime.cs`) is intentional.

### Common patterns
- `this T value` extension signatures with nullable-friendly behavior.
- Chinese XML doc comments throughout.
- Loose parsing (accept Chinese `是/否`) is the house style for boolean coercion.

## Dependencies
### Internal
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).
### External
- `System.Collections.Concurrent` (caching).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
