<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Settings

## Purpose
Optional runtime knobs passed to `SequentialGuidIDGenerator.Create`. Lets callers override the timestamp source (e.g., for deterministic tests) or switch to little-endian binary layout for databases that sort GUIDs byte-wise.

## Key Files
| File | Description |
|------|-------------|
| `SequentialGuidSettings.cs` | Sealed class with `TimeNow: DateTimeOffset?` and `LittleEndianBinary16Format: bool`. |

## For AI Agents

### Working in this directory
- Both properties are nullable / optional — leaving them at default produces wall-clock UTC, big-endian style ordering.
- Avoid adding mutation methods; this is a value-object passed per call.

### Common patterns
- Sealed with `[SuppressSniffer]`.

## Dependencies
### External
- `System` only.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
