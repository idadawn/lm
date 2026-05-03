<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Proxies

## Purpose
Aspect-style (non-`DispatchProxy`-based) interface proxy abstraction used by `Poxiao.RemoteRequest`'s `HttpDispatchProxy`. Splits responsibilities into a public abstract base (`AspectDispatchProxy` with `Invoke` / `InvokeAsync` / `InvokeAsyncT<T>`), a runtime IL generator (`AspectDispatchProxyGenerator`), and two marker interfaces that DI-aware proxies implement to expose the target instance and `IServiceProvider`.

## Key Files
| File | Description |
|------|-------------|
| `AspectDispatchProxy.cs` | Abstract class — `Create<T, TProxy>()` factory plus three abstract intercept points: synchronous `Invoke`, async `InvokeAsync`, generic-result `InvokeAsyncT<T>`. |
| `AspectDispatchProxyGenerator.cs` | IL-emit machinery that builds a runtime type implementing `T` and forwarding calls into a derived `AspectDispatchProxy`. |
| `IDispatchProxy.cs` | DI marker — `object Target { get; set; }`, `IServiceProvider Services { get; set; }`. Implemented by every proxy that needs scoped service access. |
| `IGlobalDispatchProxy.cs` | Empty marker that extends `IDispatchProxy` to flag a proxy as register-globally (consumed by `AddDispatchProxyForInterface` in DI extensions). |

## For AI Agents

### Working in this directory
- Choose `AspectDispatchProxy` (this folder) when you need split sync / async / async-generic intercept points and IL-level performance — used by `HttpDispatchProxy`. Choose `DynamicDispatchProxy` (sibling `DynamicProxies/`) when you need single-method `Invoke(Invocation)` semantics and class (non-interface) interception.
- `IDispatchProxy.Target` is normally `null` for HTTP dispatch (the interface has no real implementation); `Services` is set by `AddDispatchProxyForInterface` so retry policies, JSON providers, etc. can be resolved per call.

### Common patterns
- Generator-pair pattern: an abstract intercept base + a static `Generator` class that emits the concrete type via `Reflection.Emit`. Cached by `(serviceType, proxyType)` pair internally.

## Dependencies
### Internal
- `Poxiao.RemoteRequest.HttpDispatchProxy` is the primary consumer.

### External
- `System.Reflection.Emit`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
