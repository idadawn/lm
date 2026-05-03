<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ShortID

## Purpose
Short, URL-safe ID generator (8–14 char alphanumeric, optional special chars `_-`). Adapted from `bolorundurowb/shortid`. Used for invitation codes, share links and any user-visible identifier where a 36-char GUID is too long. Not suitable for primary keys.

## Key Files
| File | Description |
|------|-------------|
| `ShortIDGen.cs` | Static API `ShortIDGen.NextID()` / `NextID(GenerationOptions)` with thread-safe `Random`-backed character pool. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Internal/` | `Constants` and `RandomHelpers` (see `Internal/AGENTS.md`). |
| `Options/` | `GenerationOptions` configuration record (see `Options/AGENTS.md`). |

## For AI Agents

### Working in this directory
- The character pool excludes ambiguous-looking glyphs (`O`, `Z`, `i`); preserve that omission when changing `Bigs`/`Smalls`.
- The class is `static` and uses a single shared `Random` behind `ThreadLock` — do not switch to `Random.Shared` without auditing tests that seed for determinism.
- Validate `Length` against `Constants.Minimum/MaximumAutoLength` before generating.

### Common patterns
- Default call uses numbers + letters, no specials, length 8.
- Custom symbol sets: `SetCharacters(string)` (in the same file) replaces `_pool`.

## Dependencies
### Internal
- Sibling `Internal/Constants`, `Internal/RandomHelpers`, `Options/GenerationOptions`.
### External
- `System.Text`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
