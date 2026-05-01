<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Internal helpers shared between the convention, providers and feature provider. `Penetrates` holds verb-mapping tables, controller-order caches and reflection helpers; `ParameterRouteTemplate` is a small DTO the convention populates while assembling per-action route segments.

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | `internal static`: `GroupSeparator = "##"`, `VerbToHttpMethods`, `ControllerOrderCollection`, `IsApiController`, name-stripping helpers. |
| `ParameterRouteTemplate.cs` | Holds four lists (`ControllerStart/EndTemplates`, `ActionStart/EndTemplates`) keyed by `ApiSeats`. |

## For AI Agents

### Working in this directory
- Both classes are `internal` — keep them so; nothing in `api/src/modularity/**` should reference these types.
- `Penetrates.GroupSeparator` (`##`) is part of the Swagger group naming protocol — do not change without updating the convention and any documentation.
- Verb table is keyed by Chinese-alias-aware prefixes (`get`, `post`, `update`, `delete`, `add`, etc.) and ordered by length; preserve ordering when adding entries.

### Common patterns
- `ConcurrentDictionary` used for thread-safe lazy population.

## Dependencies
### Internal
- `../Attributes/`, `../Enums/ApiSeats`.
### External
- `Microsoft.AspNetCore.Mvc`, `System.Collections.Concurrent`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
