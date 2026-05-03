<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DynamicProxies

## Purpose
Async-aware enhancement of `System.Reflection.DispatchProxy`. Provides:
- A `DynamicDispatchProxy` base class that routes `Invoke(MethodInfo, args)` to abstract sync / `Task` / `Task<T>` `Invoke[Async]` hooks taking a unified `Invocation` context.
- A `ClassProxyGenerator<TClass>` that uses Roslyn (`CSharpSyntaxTree.ParseText` + `CSharpCompilation`) to **emit at runtime** a duck-typed interface and a derived class so concrete (non-virtual) types can be intercepted.

## Key Files
| File | Description |
|------|-------------|
| `DynamicDispatchProxy.cs` | Abstract class extending `DispatchProxy`. Static `Decorate<TService, TProxy>` / `DecorateClass<TClass, TProxy>` factories; overrides `Invoke` to dispatch to `Invoke(Invocation)`, `InvokeAsync(Invocation)`, or `InvokeAsync<T>(Invocation)` based on the target method's return type. |
| `Invocation.cs` | Per-call context — wraps `MethodInfo`, args, target instance and a `Properties` bag passed in at decoration time. |
| `ClassProxyGenerator.cs` | Static-constructed generic generator: scans `TClass`'s public ctors + methods, renders C# source from `DUCK_*_TEMPLATE` constants, compiles via Roslyn into an in-memory assembly, caches the resulting interface for later `DispatchProxy.Decorate`. |

## For AI Agents

### Working in this directory
- `ClassProxyGenerator<TClass>` rejects `sealed` and `abstract` classes — keep that guard; Roslyn-emitted derived classes need a non-sealed base with at least one public constructor.
- Avoid changing the `@interface_methods` / `@constructor_methods` template tokens without adjusting the generator's `Replace(...)` calls; the templates use `@`-prefixed placeholders rather than C# verbatim semantics.
- Async return-type handling lives in the `Invoke(MethodInfo, object[])` override — generic `Task<T>` is dispatched via cached `_invokeAsyncOfTMethod`. Preserve this dispatch ordering (Task → Task<T> → sync).

### Common patterns
- Per-closed-generic static caches (`_assembly`, `_interfaceType`) avoid recompilation cost on each decoration.
- Roslyn metadata loaded with `unsafe { TryGetRawMetadata(...) }` for AOT-friendly assembly references.

## Dependencies
### Internal
- `Poxiao.Reflection.Proxies` (sibling — `AspectDispatchProxy` is the alternative non-async-aware path).

### External
- `Microsoft.CodeAnalysis.CSharp` (Roslyn).
- `System.Reflection.DispatchProxy`, `System.Runtime.Loader`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
