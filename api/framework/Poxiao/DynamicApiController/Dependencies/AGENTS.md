<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dependencies

## Purpose
The marker interface that identifies a class as an auto-exposed API. Empty by design; the scanner in `../Conventions/` and `../Providers/` keys on it.

## Key Files
| File | Description |
|------|-------------|
| `IDynamicApiController.cs` | Empty interface; implementations become MVC controllers without inheriting `ControllerBase`. |

## For AI Agents

### Working in this directory
- Keep this interface empty — adding members forces every service across `api/src/modularity/**` to implement them.
- Either implement `IDynamicApiController` or annotate with `[DynamicApiController]` from `../Attributes/`; both are honoured by the feature provider.

### Common patterns
- LIMS application services usually combine `IDynamicApiController` with a lifetime marker from `Poxiao.DependencyInjection.Dependencies` (e.g., `IScoped`).

## Dependencies
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
