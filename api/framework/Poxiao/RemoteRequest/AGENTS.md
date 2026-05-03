<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# RemoteRequest

## Purpose
Declarative HTTP client (Refit-style). Define a C# interface, decorate methods with `[Get("/api/foo")]` / `[Post]` / `[Put]` / `[Delete]` etc. and parameters with `[Body]` / `[QueryString]` / `[Headers]`; `services.AddRemoteRequest()` wires up an `IHttpDispatchProxy`-derived proxy that uses `IHttpClientFactory` to dispatch the request, applying interceptors, retry policies and pluggable JSON serializers along the way.

## Key Files
| File | Description |
|------|-------------|
| `Http.cs` | Static façade — `Http.GetHttpProxy<TInterface>()` resolves the proxy from `App.RootServices`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | All declarative attributes — clients, methods, headers, body, retry, interceptors, JSON config (see `Attributes/AGENTS.md`) |
| `Enums/` | `InterceptorTypes` enum (see `Enums/AGENTS.md`) |
| `Events/` | `HttpRequestFaildedEventArgs` (see `Events/AGENTS.md`) |
| `Extensions/` | DI registration + `HttpRequestMessage`/`HttpResponseMessage`/`string` helpers (see `Extensions/AGENTS.md`) |
| `Internal/` | `HttpRequestPart` builder, `HttpFile` upload model, methods/setters partials (see `Internal/AGENTS.md`) |
| `Proxies/` | `IHttpDispatchProxy` marker + `HttpDispatchProxy` aspect implementation (see `Proxies/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Sync calls are explicitly disallowed — `HttpDispatchProxy.Invoke` throws `NotSupportedException("Please use asynchronous operation mode.")`. Define your interface methods as `Task` / `Task<T>` only.
- `[Client("foo")]` selects a named `HttpClient` (configured via standard `IHttpClientFactory`); without it the default unnamed client is used (registered in `RemoteRequestServiceCollectionExtensions` with `ServerCertificateCustomValidationCallback => true` and a 5-minute handler lifetime — be aware of this for prod hosts).
- New parameter attributes must derive from `ParameterBaseAttribute` so they are picked up by `HttpRequestPart` argument scanning.

### Common patterns
- Interface → proxy: `AddDispatchProxyForInterface<HttpDispatchProxy, IHttpDispatchProxy>(typeof(ISingleton))` registers any user interface that extends `IHttpDispatchProxy` as a singleton.
- Per-call mutable state lives in `HttpRequestPart` (a `partial class` split across `Internal/HttpRequestPart{,Methods,Setters}.cs`).

## Dependencies
### Internal
- `Poxiao.Reflection.Proxies` (`AspectDispatchProxy`, `IDispatchProxy`).
- `Poxiao.JsonSerialization` (pluggable JSON providers).
- `Poxiao.DataValidation` (`[ValidationData]`-style integration on parameters).

### External
- `IHttpClientFactory`, `Microsoft.Extensions.DependencyInjection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
