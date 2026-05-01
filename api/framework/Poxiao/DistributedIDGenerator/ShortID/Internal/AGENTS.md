<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Implementation helpers shared inside the `ShortID` generator. Holds the public length constants used to validate `GenerationOptions.Length`, and a tiny thread-safe wrapper around `System.Random` so multiple threads calling `ShortIDGen.NextID()` don't corrupt the PRNG state.

## Key Files
| File | Description |
|------|-------------|
| `Constants.cs` | `MinimumAutoLength = 8`, `MaximumAutoLength = 14`, `MinimumCharacterSetLength = 50`. |
| `RandomHelpers.cs` | `GenerateNumberInRange(min, max)` guarded by an internal `lock`. |

## For AI Agents

### Working in this directory
- Both classes are `internal static` — keep visibility as-is; nothing outside `ShortID/` should depend on them.
- Changing `Maximum/MinimumAutoLength` widens or breaks every existing short ID; treat as a versioned migration if ever required.

### Common patterns
- `lock (ThreadLock)` around all `Random.Next(...)` calls.

## Dependencies
### External
- `System` only.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
