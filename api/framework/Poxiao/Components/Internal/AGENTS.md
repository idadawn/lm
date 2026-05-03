<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Internal implementation detail for the Components feature: the `Penetrates` static helper that builds an ordered dependency link of components from a root component type by reflecting `[DependsOn]` attributes. It detects self-reference and circular references and constructs the parallel `ComponentContext` chain that the public extensions later iterate over.

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | `internal static` helper. `CreateDependLinkList(Type, object)` returns a `List<ComponentContext>` for the public `AddComponent` / `UseComponent` extensions; recursive overload accumulates `dependLinkList` + `componentContextLinkList`, applies `[DependsOn]` `DependComponents` (real graph) and `LinkComponents` (sibling activation), throws `InvalidOperationException` on self-/cycle references. |

## For AI Agents

### Working in this directory
- Keep this type `internal` and named `Penetrates` — convention is shared across all Poxiao framework features (see `CorsAccessor/Internal`, `ConfigurableOptions/Internal`, etc.).
- Cycle detection currently throws `"There is a circular reference problem between components."` — keep messaging stable; downstream tools may match on it.
- The recursive walker mutates `dependLinkList` and `componentContextLinkList` by `ref`; if you refactor toward immutability, ensure the same insertion order is preserved (root last is significant for `Activator.CreateInstance` ordering).

### Common patterns
- "Penetrates" naming for module-private helpers.
- Reflection over `DependsOnAttribute` rather than constructor injection.

## Dependencies
### Internal
- `Components/Contexts/ComponentContext.cs`, `Components/Dependencies/IComponent.cs`.
### External
- `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
