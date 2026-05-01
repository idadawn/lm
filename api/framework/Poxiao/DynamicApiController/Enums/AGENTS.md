<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
Enum that identifies where a route-template parameter is rendered relative to the controller / action segment. Consumed by `[ApiSeat]` and the convention's `ParameterRouteTemplate` builder.

## Key Files
| File | Description |
|------|-------------|
| `ApiSeats.cs` | `ControllerStart`, `ControllerEnd`, `ActionStart`, `ActionEnd`. |

## For AI Agents

### Working in this directory
- Adding a new seat requires extending `../Internal/ParameterRouteTemplate` and the convention rewriter symmetrically.
- Each member has a Chinese `[Description]` shown in admin tooling.

### Common patterns
- `[SuppressSniffer]`; lives in the `Microsoft.AspNetCore.Mvc` namespace by convention.

## Dependencies
### External
- `System.ComponentModel`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
