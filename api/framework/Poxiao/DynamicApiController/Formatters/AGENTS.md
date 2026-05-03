<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Formatters

## Purpose
Custom MVC input formatter that lets dynamic API actions accept `text/plain` request bodies (UTF-8 / Unicode). Useful for endpoints receiving raw strings, JWT bodies or webhook payloads that don't ship JSON content-type.

## Key Files
| File | Description |
|------|-------------|
| `TextPlainMediaTypeFormatter.cs` | Sealed `TextInputFormatter` registering `text/plain` and reading the body via `StreamReader.ReadToEndAsync`. |

## For AI Agents

### Working in this directory
- Registered automatically by `../Extensions/AddDynamicApiControllers`. Do not register again from feature modules.
- Class is `sealed`; if a new content-type is needed, add a sibling formatter rather than extending this one.
- Encoding list is `UTF-8` and `Unicode` only — Big5 / GB18030 are intentionally excluded.

### Common patterns
- Uses `Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse` for header construction.

## Dependencies
### External
- `Microsoft.AspNetCore.Mvc.Formatters`, `Microsoft.Net.Http.Headers`, `System.Text`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
