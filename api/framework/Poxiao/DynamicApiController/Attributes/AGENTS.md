<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
Attribute set used to declare and tune dynamic API controllers. `[DynamicApiController]` marks a class as a controller without implementing the `IDynamicApiController` interface; `[ApiDescriptionSettings]` controls Swagger group, version, name and ignore-state; `[ApiSeat]` and `[RouteConstraint]` shape the synthesised route template; `[QueryParameters]` forces all action parameters to `[FromQuery]`.

## Key Files
| File | Description |
|------|-------------|
| `DynamicApiControllerAttribute.cs` | Class-level marker, sealed, no properties. |
| `ApiDescriptionSettingsAttribute.cs` | Inherits `ApiExplorerSettingsAttribute`, adds `Tag`, `Module`, `Order`, `KeepName`, `KeepVerb`, `SplitCamelCase` etc. |
| `ApiSeatAttribute.cs` | Parameter-level: `Seat: ApiSeats` (ControllerStart/End, ActionStart/End). |
| `QueryParametersAttribute.cs` | Method-level marker that converts every parameter to `[FromQuery]`. |
| `RouteConstraintAttribute.cs` | Parameter-level: `Constraint` string injected into the route template (e.g. `int`, `regex(...)`). |

## For AI Agents

### Working in this directory
- These attributes are framework primitives — used by the `Conventions/` rewriter; renaming a property breaks every consumer in `api/src/modularity/**`.
- `ApiDescriptionSettingsAttribute` deliberately extends MVC's `ApiExplorerSettingsAttribute`; do not break that base relationship.
- `[ApiSeat]` defaults to `ApiSeats.ActionEnd`.

### Common patterns
- All decorated `[SuppressSniffer]` and live in either `Microsoft.AspNetCore.Mvc` (so consumers get them via the standard MVC `using`) or `Poxiao.DynamicApiController`.

## Dependencies
### Internal
- `../Enums/ApiSeats`.
### External
- `Microsoft.AspNetCore.Mvc`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
