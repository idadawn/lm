<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
DI registration entry point for the Mapster-based object mapper. Scans caller-supplied assemblies for `IRegister` mapping configurations, applies the framework's default name-matching strategy, and registers `IMapper` (Mapster `ServiceMapper`).

## Key Files
| File | Description |
|------|-------------|
| `ObjectMapperServiceCollectionExtensions.cs` | `AddObjectMapper(IServiceCollection, params Assembly[])` — `Scan` for `IRegister`, configure `NameMatchingStrategy` (`Flexible` → `IgnoreCase`) with `PreserveReference(true)`. |

## For AI Agents

### Working in this directory
- Assembly list is `params Assembly[]`; pass module assemblies (e.g., `Poxiao.Apps`, `Poxiao.AI`) so their `IRegister` classes are picked up.
- Two `Default` configures are intentional — the second overwrites the strategy to `IgnoreCase` while keeping `PreserveReference`. Don't "deduplicate" without re-checking Mapster semantics.
- Live in the `Microsoft.Extensions.DependencyInjection` namespace.

### Common patterns
- Single-line registration call from API entry point; keep this signature stable.

## Dependencies
### External
- `Mapster`, `MapsterMapper`, `Microsoft.Extensions.DependencyInjection.Abstractions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
