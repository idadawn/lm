<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Configuration record passed to `ShortIDGen.NextID(options)` to tune length and character set.

## Key Files
| File | Description |
|------|-------------|
| `GenerationOptions.cs` | `UseNumbers` (default false), `UseSpecialCharacters` (default true), `Length` (random within `Constants.Minimum/MaximumAutoLength`). |

## For AI Agents

### Working in this directory
- The default `Length` is computed at construction time via `RandomHelpers.GenerateNumberInRange` — instantiating the same options twice can yield different defaults; pin `Length` explicitly when reproducibility matters.
- Defaults differ from the public `ShortIDGen.NextID()` no-arg helper (which forces `UseNumbers = true`, `UseSpecialCharacters = false`); be aware when constructing manually.

### Common patterns
- Decorated `[SuppressSniffer]`.

## Dependencies
### Internal
- `../Internal/Constants`, `../Internal/RandomHelpers`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
