<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Conventions

## Purpose
The MVC `IApplicationModelConvention` that turns a `IDynamicApiController` service into an HTTP controller at startup: rewrites controller name (strip `Service`/`AppService` suffix, apply route prefix and module), assigns HTTP verbs from method-name prefixes, builds parameter route templates from `[ApiSeat]` / `[RouteConstraint]`, and applies group / order metadata from `[ApiDescriptionSettings]`.

## Key Files
| File | Description |
|------|-------------|
| `DynamicApiControllerApplicationModelConvention.cs` | Internal sealed convention reading `DynamicApiControllerSettingsOptions`, regex-matching versioned names, mutating each `ControllerModel` / `ActionModel` / `ParameterModel`. |

## For AI Agents

### Working in this directory
- Single most complex file in the module — touch carefully and run the API in dev to verify Swagger output before/after.
- Honour `KeepName`, `KeepVerb`, `LowercaseRoute`, `AsLowerCamelCase` settings; never hard-code casing.
- Do not introduce business-specific naming heuristics here; those belong in `Options/` or `[ApiDescriptionSettings]` overrides.

### Common patterns
- Uses `Penetrates.VerbToHttpMethods` and `Penetrates.ControllerOrderCollection` from `../Internal/`.
- Returns `IDispatchProxy`-aware shapes so DI proxies still bind correctly.

## Dependencies
### Internal
- `../Options/DynamicApiControllerSettingsOptions`, `../Internal/Penetrates`, `Poxiao.UnifyResult`.
### External
- `Microsoft.AspNetCore.Mvc.ApplicationModels`, `System.Text.RegularExpressions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
