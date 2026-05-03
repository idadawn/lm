<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
DI registration + helpers around `HttpRequestMessage`, `HttpResponseMessage`, and URL/string composition for the declarative HTTP pipeline.

## Key Files
| File | Description |
|------|-------------|
| `RemoteRequestServiceCollectionExtensions.cs` | `services.AddRemoteRequest(configure?, includeDefaultHttpClient=true)` — registers the `HttpDispatchProxy` against `IHttpDispatchProxy` as a singleton (via `AddDispatchProxyForInterface`), creates the default unnamed `HttpClient` (5-min handler lifetime, accepts any TLS cert), and lets the caller add named clients in `configure`. Also exposes `ApproveAllCerts()` (process-wide insecure TLS — flagged "慎用"). |
| `HttpRequestMessageExtensions.cs` | Mutators / readers on `HttpRequestMessage` used by the pipeline (URL templating, header copy, etc.). |
| `HttpResponseMessageExtensions.cs` | Response readers — content negotiation, `ReadAsStringAsync`/`ReadAsObjectAsync<T>` style helpers consistent with the JSON-provider abstraction. |
| `RemoteRequestStringExtensions.cs` | URL/query-string composition helpers (escape, template substitution). |

## For AI Agents

### Working in this directory
- Default client uses `ServerCertificateCustomValidationCallback = (_,_,_,_) => true`. This is a deliberate dev convenience but a security smell in production — flag any change here in code review.
- Adding a new HttpClient configuration shouldn't go in `AddRemoteRequest`'s default branch; pass via the `configure` delegate so apps stay opt-in.

### Common patterns
- All public extensions are `[SuppressSniffer]` static classes so they aren't auto-registered by Poxiao's DI scanner.

## Dependencies
### Internal
- `Poxiao.RemoteRequest.Proxies` (`HttpDispatchProxy`, `IHttpDispatchProxy`).
- DI dispatch-proxy registration helpers from `Poxiao.Reflection.Proxies`.

### External
- `IHttpClientFactory`, `Microsoft.Extensions.DependencyInjection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
