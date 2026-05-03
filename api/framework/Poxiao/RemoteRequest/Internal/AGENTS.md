<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
The mutable per-call request model used by `HttpDispatchProxy`. `HttpRequestPart` is a `sealed partial class` split across three files (the model itself, fluent setters, and the `SendAsync` / `SendAsAsync<T>` execution methods). Plus the multi-part file upload model and an internal equality comparer for repeat keys (used to merge duplicate header/query keys).

## Key Files
| File | Description |
|------|-------------|
| `HttpRequestPart.cs` | Properties: `RequestUrl`, `Templates`, `Method`, `Headers`, `Queries`, `ClientName`/`ClientProvider`, `Body`, `ContentType`, `ContentEncoding`, `Files`, JSON provider, retry/interceptor lists, etc. Static `Default()` factory. |
| `HttpRequestPartMethods.cs` | Execution side — `SendAsync()` returning `HttpResponseMessage`, generic `SendAsAsync<T>()`, exception-handling and retry loop. |
| `HttpRequestPartSetters.cs` | Fluent `SetUrl`, `SetMethod`, `SetHeaders`, `SetQueries`, `SetBody`, `SetClient`, `SetContentType`, `SetFiles`, etc. — used both by attribute scanning and by user code calling the static `Http` helpers. |
| `HttpFile.cs` | Form-data file payload — `Create(name, byte[]/Stream, fileName?)` factories, `CreateMultiple` for batch upload, `Name`/`FileName`/`Bytes`/`FileStream`. |
| `RepeatKeyEqualityComparer.cs` | `IEqualityComparer<string>` used when merging dictionaries that may contain duplicate keys (case-insensitive header/query merging). |

## For AI Agents

### Working in this directory
- `HttpRequestPart` is `sealed partial` — keep the split (model / methods / setters) when adding new properties: the corresponding `Set*` chainable mutator goes in `HttpRequestPartSetters.cs`, the actual HTTP work in `HttpRequestPartMethods.cs`.
- Setter methods return `this HttpRequestPart` so fluent chains work — preserve this signature.
- `HttpFile` exposes both `Bytes` and `FileStream`; only one will be set per file. Consumers must check both. Don't add a third "path" property without updating `HttpRequestPartMethods` content-building logic.

### Common patterns
- "Builder via mutable model" rather than immutable record — single instance flows through proxy → interceptors → send.

## Dependencies
### Internal
- `Poxiao.JsonSerialization` (`IJsonSerializerProvider`).

### External
- `System.Net.Http`, `System.Text` (`Encoding`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
