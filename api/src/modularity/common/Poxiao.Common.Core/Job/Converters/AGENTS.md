<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Converters

## Purpose
`System.Text.Json` converters used by the dynamic Job pipeline. Job message bodies are serialised with `Penetrates.GetDefaultJsonSerializerOptions()`, which registers the converters in this folder.

## Key Files
| File | Description |
|------|-------------|
| `DateTimeJsonConverter.cs` | `internal sealed JsonConverter<DateTime>`. Reads/writes `DateTime` via `DateTime.Parse(reader.GetString())` and `value.ToString()` (default culture-formatted). Used so HTTP-job messages survive a round-trip without timezone surprises. |

## For AI Agents

### Working in this directory
- Keep converters `internal sealed` — they are wired exclusively from `Penetrates.GetDefaultJsonSerializerOptions()`.
- If you need a different format, prefer adding a new converter and registering it in `Penetrates` rather than tweaking this one.

## Dependencies
### External
- `System.Text.Json` (`JsonConverter<T>`, `Utf8JsonReader`, `Utf8JsonWriter`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
