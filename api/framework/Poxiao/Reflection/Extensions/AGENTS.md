<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
`MethodInfo` extensions that resolve attributes from the **actual implementation** (the target object's runtime type) instead of from the interface method that a proxy receives. Used inside dispatch proxies (`HttpDispatchProxy`, `AspectDispatchProxy` derivatives) so that attributes such as `[Get]`, `[Headers]`, `[RetryPolicy]` declared on the implementation surface for inspection.

## Key Files
| File | Description |
|------|-------------|
| `MethodInfoExtensions.cs` | `GetActualCustomAttribute[s]<TAttribute>(this MethodInfo method, object target, ...)` — finds the matching method on `target.GetType()` by `MethodInfo.ToString()` equality and returns its `CustomAttribute(s)`. Multiple overloads cover `Type`, generic, `inherit` and untyped variants. |

## For AI Agents

### Working in this directory
- The match key is `MethodInfo.ToString()`, which includes return type + parameter types. Preserve this — name-only matching would alias overloaded methods.
- `target == null` returns `default` rather than throwing; callers in proxy code rely on that null-tolerance.

## Dependencies
### Internal
- Consumed by `RemoteRequest/Proxies/HttpDispatchProxy.cs` and other proxy-style infrastructure.

### External
- `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
