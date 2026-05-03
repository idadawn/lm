<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DynamicApiController

## Purpose
The Furion-style "interface as REST API" engine. Any service implementing `IDynamicApiController` (or marked `[DynamicApiController]`) is automatically wrapped in an MVC controller at startup: routes, HTTP verbs, parameter binding and grouping are all derived from the type/method name plus attribute overrides. Eliminates boilerplate Controller classes for the entire `api/src/modularity/**` tree and powers hot-reload via `IDynamicApiRuntimeChangeProvider`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[DynamicApiController]`, `[ApiDescriptionSettings]`, `[ApiSeat]`, `[QueryParameters]`, `[RouteConstraint]` (see `Attributes/AGENTS.md`). |
| `Conventions/` | The `IApplicationModelConvention` that rewrites action models (see `Conventions/AGENTS.md`). |
| `Dependencies/` | `IDynamicApiController` marker interface (see `Dependencies/AGENTS.md`). |
| `Enums/` | `ApiSeats` parameter-position enum (see `Enums/AGENTS.md`). |
| `Extensions/` | `AddDynamicApiControllers` MVC builder extension (see `Extensions/AGENTS.md`). |
| `Formatters/` | `text/plain` body formatter (see `Formatters/AGENTS.md`). |
| `Internal/` | `Penetrates` shared helpers and `ParameterRouteTemplate` (see `Internal/AGENTS.md`). |
| `Options/` | `DynamicApiControllerSettingsOptions` (see `Options/AGENTS.md`). |
| `Providers/` | `ControllerFeatureProvider` and action-descriptor change-token (see `Providers/AGENTS.md`). |
| `Runtimes/` | Hot-reload `IDynamicApiRuntimeChangeProvider` (see `Runtimes/AGENTS.md`). |

## For AI Agents

### Working in this directory
- LIMS modules (`api/src/modularity/lab`, `system`, etc.) write services not controllers; do not introduce manual `Controller` classes for endpoints already covered by dynamic API.
- HTTP verb is inferred from method-name prefix (`Get*`, `Post*`, `Update*`, `Delete*`); use `[HttpGet]` / `[HttpPost]` only when overriding.
- Use `[QueryParameters]` to force all action params to `[FromQuery]`; useful for GET endpoints with multiple primitives.

### Common patterns
- Convention layer (`Conventions/`) and feature provider (`Providers/`) cooperate: the provider decides "is this a controller", the convention decides "what does it look like".
- Settings flow from `DynamicApiControllerSettingsOptions` bound to `appsettings.json`.

## Dependencies
### Internal
- `Poxiao` core, `Poxiao.UnifyResult` (response wrapping), `Poxiao.DependencyInjection`.
### External
- `Microsoft.AspNetCore.Mvc.*` (ApplicationModels, Controllers, Formatters).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
