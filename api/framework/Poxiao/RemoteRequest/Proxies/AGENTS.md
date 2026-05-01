<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Proxies

## Purpose
The runtime proxy that turns interface method calls into HTTP requests. Any user interface that extends the marker `IHttpDispatchProxy` is registered (by `AddRemoteRequest` → `AddDispatchProxyForInterface`) such that calls dispatch into `HttpDispatchProxy`, which builds an `HttpRequestPart` from the method's attributes and arguments and sends it.

## Key Files
| File | Description |
|------|-------------|
| `IHttpDispatchProxy.cs` | Empty marker interface. User-defined remote contracts inherit it; the framework discovers all `IHttpDispatchProxy`-deriving interfaces and registers a transparent proxy for each. |
| `HttpDispatchProxy.cs` | Concrete proxy extending `AspectDispatchProxy` and implementing `IDispatchProxy` (`Target`, `Services`). Synchronous `Invoke` throws `NotSupportedException` (async-only API). `InvokeAsync` and `InvokeAsyncT<T>` build a `HttpRequestPart` via `BuildHttpRequestPart(method, args)` and call `SendAsync` / `SendAsAsync<T>`. |

## For AI Agents

### Working in this directory
- Sync calls are intentionally rejected at runtime. Don't "add a synchronous fallback" — the rest of the codebase relies on every remote-request method being awaitable.
- `BuildHttpRequestPart` performs all attribute scanning (`[Get]`, `[Headers]`, `[Body]`, `[QueryString]`, `[RetryPolicy]`, `[JsonSerialization]`, `[Interceptor]`, `[Client]`). When adding a new attribute family, that's the function to extend (it lives lower in the file, ~line 60+).
- `Services` is set via `IDispatchProxy` by the dispatch-proxy generator and is the source of truth for resolving `IHttpClientFactory`, JSON providers, and validators.

### Common patterns
- Aspect-style proxy (not BCL `DispatchProxy`) for split sync/async/async-generic intercept points.
- Performance note in the source: "以下代码还需进一步优化性能，启动时把所有扫描缓存起来" — attribute scanning currently runs per call; a startup-time cache is a planned optimisation.

## Dependencies
### Internal
- `Poxiao.Reflection.Proxies` (`AspectDispatchProxy`, `IDispatchProxy`).
- `Poxiao.Reflection.Extensions` (`MethodInfoExtensions.GetActualCustomAttribute`).
- `Poxiao.RemoteRequest.Internal.HttpRequestPart`.
- `Poxiao.JsonSerialization`, `Poxiao.DataValidation`, `Poxiao.ClayObject`.

### External
- `System.Net.Http`, `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
