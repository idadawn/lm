<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Cross-cutting helpers private to the Job runtime — JSON serialisation options, naming-convention conversion, JsonElement value coercion, time normalisation. Kept `internal static` so they don't leak into other modules' surface API.

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | `internal static` helper. `GetDefaultJsonSerializerOptions()` returns `PropertyNameCaseInsensitive=true`, `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`, registers `DateTimeJsonConverter`. Also exposes `Serialize`/`Deserialize`, `GetNowTime(useUtc)`, `GetUnspecifiedTime`, `SplitToWords`, `GetNaming(propertyName, NamingConventions)` (CamelCase/Pascal/UnderScoreCase), `GetJsonElementValue` (recursive JsonElement → primitive), `LoadAssembly`, `Write(Action<Utf8JsonWriter>)`. |

## For AI Agents

### Working in this directory
- Members are intentionally `internal` — do not promote to `public`. If another assembly needs the same helper, copy it or expose a slim public wrapper.
- `GetUnspecifiedTime` truncates below the millisecond on purpose (`new DateTime(year, month, day, hour, minute, second, millisecond)`); preserves Quartz-style timing accuracy.
- `GetNaming` enums are `Poxiao.Schedule.NamingConventions` — keep the enum reference if it moves.

## Dependencies
### Internal
- `Poxiao.Schedule.NamingConventions`, sibling `Converters/DateTimeJsonConverter`.

### External
- `System.Text.Json`, `System.Text.RegularExpressions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
