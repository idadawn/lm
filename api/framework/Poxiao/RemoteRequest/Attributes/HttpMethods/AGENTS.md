<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HttpMethods

## Purpose
Per-verb method attributes (`[Get]`, `[Post]`, `[Put]`, `[Patch]`, `[Delete]`, `[Head]`) for declarative HTTP interfaces, plus the shared `HttpMethodBaseAttribute` that holds verb-agnostic settings (URL, content type, encoding, GZip, HTTP version, URL escaping, null-query handling).

## Key Files
| File | Description |
|------|-------------|
| `HttpMethodBaseAttribute.cs` | Base — properties: `RequestUrl`, `Method` (`HttpMethod`), `WithGZip`, `ContentType` (default `application/json`), `Encoding` (default UTF-8), `WithEncodeUrl`, `IgnoreNullValueQueries`, `HttpVersion` (default `1.1`). |
| `GetAttribute.cs` | `[Get]` / `[Get("/api/items")]` — passes `HttpMethod.Get` to base. |
| `PostAttribute.cs` | `[Post]` — `HttpMethod.Post`. |
| `PutAttribute.cs` | `[Put]` — `HttpMethod.Put`. |
| `PatchAttribute.cs` | `[Patch]` — `HttpMethod.Patch`. |
| `DeleteAttribute.cs` | `[Delete]` — `HttpMethod.Delete`. |
| `HeadAttribute.cs` | `[Head]` — `HttpMethod.Head`. |

## For AI Agents

### Working in this directory
- New verbs (e.g. `Options`, custom `Trace`) should derive from `HttpMethodBaseAttribute` exactly like the existing ones — pass the verb in via the protected-equivalent constructor.
- Defaults on the base class are deliberate (`application/json`, UTF-8, HTTP/1.1, URL-escape on, GZip off). Don't change defaults without sweeping callers — many module clients rely on them.

### Common patterns
- Two-ctor convention on every derivative: parameterless + `(string requestUrl)`.

## Dependencies
### External
- `System.Net.Http.HttpMethod`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
