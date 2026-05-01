<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
Declarative attributes that fine-tune Poxiao DI scanning. `[Injection]` controls how a discovered class is registered (action, pattern, named alias, order, except-interface list); `[SuppressProxy]` opts a class out of dispatch-proxy interception; `[SuppressSniffer]` hides infrastructure types from logging/diagnostics sniffers.

## Key Files
| File | Description |
|------|-------------|
| `InjectionAttribute.cs` | Class-level attribute carrying `InjectionActions`, `InjectionPatterns`, `Named`, `Order`, `ExceptInterfaces`. |
| `SuppressProxyAttribute.cs` | Marks a service to bypass `AspectDispatchProxy` wrapping during registration. |
| `SuppressSnifferAttribute.cs` | Marks a type as framework infrastructure so reflective scanners skip it. |

## For AI Agents

### Working in this directory
- These attributes are read by the scanner in `../Extensions/DependencyInjectionServiceCollectionExtensions.cs` — keep their property names stable; renaming is a breaking change for every consumer in `api/src/modularity/**`.
- Default values matter: `Pattern = InjectionPatterns.All`, `Action = InjectionActions.Add`. Don't change defaults silently.

### Common patterns
- All three attributes carry `[SuppressSniffer]` themselves so they don't recurse during scanning.
- `InjectionAttribute` is `AttributeUsage(AttributeTargets.Class)`; the others follow the same scope.

## Dependencies
### Internal
- Sibling `../Enums/` for `InjectionActions` and `InjectionPatterns`.
### External
- None beyond `System`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
