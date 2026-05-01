<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Contexts

## Purpose
Holds the runtime context object passed through the Poxiao Components dependency-link engine. The context tracks each component's type, its parent / root references, depend / link metadata and a shared property bag — used by `IServiceComponent.Load` and `IApplicationComponent.Load` so a component can read its parameters and reach values published by upstream components.

## Key Files
| File | Description |
|------|-------------|
| `ComponentContext.cs` | Sealed `[SuppressSniffer]` class exposing `ComponentType`, `CalledContext`, `RootContext`, `DependComponents`, `LinkComponents` plus typed `SetProperty<TComponent>(value)` / `GetProperty<TComponent,TOptions>()` helpers. Properties are stored on the root context so the whole link list shares one bag. |

## For AI Agents

### Working in this directory
- Do not change the public API surface lightly — `ComponentContext` is the contract every `IServiceComponent` / `IApplicationComponent` consumes (see `Components/Extensions`).
- Property keys are derived from `componentType.FullName`; keep that scheme when adding overloads, otherwise downstream `GetProperty` calls miss.
- Internal setters (`ComponentType`, `CalledContext`, `RootContext`, `DependComponents`, `LinkComponents`, `IsRoot`) are populated only by `Components/Internal/Penetrates.cs` — leave them `internal`.

### Common patterns
- Root-vs-nested context distinction via `IsRoot` and `RootContext`.
- Bag indirection: nested contexts read/write through `RootContext.Properties` to keep one shared dictionary.

## Dependencies
### Internal
- `Components/Dependencies/IComponent.cs` — generic constraints on context helpers.
- `Components/Internal/Penetrates.cs` — populates context fields when building the depend link list.

### External
- BCL only (`System`, `System.ComponentModel`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
