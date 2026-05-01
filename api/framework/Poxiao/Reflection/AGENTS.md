<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Reflection

## Purpose
Reflection / dynamic-proxy infrastructure shared by the rest of Poxiao. Two responsibilities live here:
1. **Type & assembly resolution helpers** — internal `Reflect` static class wrapping `AssemblyLoadContext`, `Assembly.Load`, `GetType`.
2. **Dynamic dispatch proxies** — base classes used by `RemoteRequest` (HTTP client) and other AOP-style features to intercept interface calls, including async-aware `Task` / `Task<T>` handling that the BCL's `DispatchProxy` does not natively support.

## Key Files
| File | Description |
|------|-------------|
| `Reflect.cs` | `internal static` helpers: `GetEntryAssembly`, `GetAssembly(name)`, `LoadAssembly(path|stream)`, `GetType(assembly, fullName)`, `GetStringType("Asm;Ns.Type")`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `DynamicProxies/` | Roslyn-codegen class proxies + async-friendly `DynamicDispatchProxy` (see `DynamicProxies/AGENTS.md`) |
| `Extensions/` | `MethodInfo` extensions to fetch attributes from the *implementation* method, not the interface (see `Extensions/AGENTS.md`) |
| `Internal/` | Internal helper types — parameter-info wrappers (see `Internal/AGENTS.md`) |
| `Proxies/` | `AspectDispatchProxy`, `IDispatchProxy`, `IGlobalDispatchProxy` (see `Proxies/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `Reflect` is `internal` — do not expose it. External callers should go through public extension surfaces (`App.Assemblies`, `ObjectMapperServiceCollectionExtensions`, etc.).
- The two proxy stacks (`AspectDispatchProxy` / `Proxies/` and `DynamicDispatchProxy` / `DynamicProxies/`) are intentionally separate: aspect-style is used by `RemoteRequest`'s `HttpDispatchProxy`; the dynamic one supports `Decorate<TService, TProxy>` and `DecorateClass<TClass, TProxy>` for AOP on **concrete** classes via Roslyn-emitted duck interfaces.

### Common patterns
- `static` cache fields on generic classes (`ClassProxyGenerator<T>`) to memoise generated proxy assemblies/interfaces per closed type.

## Dependencies
### Internal
- Consumed by `RemoteRequest`, `ObjectMapper`, scheduling, and module-loader code paths.

### External
- `Microsoft.CodeAnalysis.CSharp` (Roslyn) — for runtime C# compilation in `ClassProxyGenerator`.
- `System.Reflection.DispatchProxy`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
