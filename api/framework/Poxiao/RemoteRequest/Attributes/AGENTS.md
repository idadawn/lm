<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
The full declarative-HTTP attribute surface. Applied to interfaces / methods / parameters of an `IHttpDispatchProxy`-derived service contract; consumed by `HttpDispatchProxy.BuildHttpRequestPart` to translate a method call into an `HttpRequestMessage`.

## Key Files
| File | Description |
|------|-------------|
| `ClientAttribute.cs` | `[Client("name")]` on interface/method — picks a named `HttpClient` from `IHttpClientFactory`. |
| `HeadersAttribute.cs` | `[Headers("X-Foo", "value")]` (interface/method) **or** `[Headers]` / `[Headers("alias")]` on a parameter to forward the value as a header. `AllowMultiple = true`. |
| `RetryPolicyAttribute.cs` | `[RetryPolicy(numRetries, retryTimeout=1000ms)]` on interface/method. |
| `InterceptorAttribute.cs` | `[Interceptor(InterceptorTypes.Request)]` etc. — on a static method makes it a global interceptor; on a parameter binds an `Action<HttpClient>` / `Action<HttpRequestMessage>` etc. supplied by the caller for that one call. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `HttpMethods/` | `[Get]`, `[Post]`, `[Put]`, `[Patch]`, `[Delete]`, `[Head]` plus the `HttpMethodBaseAttribute` they all derive from (see `HttpMethods/AGENTS.md`) |
| `JsonSerialization/` | Pluggable JSON provider + `JsonSerializerOptions` overrides (see `JsonSerialization/AGENTS.md`) |
| `Parameters/` | `[Body]`, `[QueryString]`, `ParameterBaseAttribute` base (see `Parameters/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Most attributes are dual-target (`AttributeTargets.Interface | Method | Parameter`). When adding new ones follow the same convention so users can apply at whichever scope makes sense.
- All public attributes carry `[SuppressSniffer]` so the framework's auto-DI assembly scanner does not treat them as service registrations. Keep this on new attributes.

### Common patterns
- `Method`-targeted base + thin per-verb derivatives (mirrors `HttpMethodBaseAttribute` → `GetAttribute` etc.).

## Dependencies
### External
- `System.Net.Http` (`HttpMethod`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
