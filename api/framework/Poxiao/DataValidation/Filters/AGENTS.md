<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Filters

## Purpose
MVC / Razor Pages action filters that turn model-state failures and `AppFriendlyException(ValidationException=true)` into the LIMS unified result envelope. Registered globally by `AddDataValidation` with a high priority (`Order = -1000`).

## Key Files
| File | Description |
|------|-------------|
| `DataValidationFilter.cs` | `IAsyncActionFilter, IOrderedFilter`. Skips when WebSocket, `[NonValidation]` on method or class, no parameters, OData controllers, or already-set `Result`. On failure, builds a `ValidationMetadata` via `ValidatorContext`, stores it in `HttpContext.Items`, and either returns `JsonResult` / `BadPageResult` (when unify is bypassed) or delegates to `IUnifyResultProvider.OnValidateFailed`. Also catches post-action `AppFriendlyException` flagged `ValidationException`. |
| `DataValidationPageFilter.cs` | Same idea for Razor Pages handlers — peers with the MVC filter. |

## For AI Agents

### Working in this directory
- `Order = -1000` is intentional so this filter runs before authorization-class filters that might short-circuit; preserve.
- The dynamic `finalContext = resultContext != null ? resultContext : context` is intentional to write `Result` on either the executing or executed context — keep `dynamic` rather than picking a single static type.
- Storage keys in `HttpContext.Items` are `nameof(DataValidationFilter)+nameof(...)` — other middleware (the unified result, MiniProfiler) reads them; don't rename without updating consumers.

### Common patterns
- Two-phase handling: pre-execution model-state check + post-execution `AppFriendlyException` recovery.
- `UnifyContext.CheckFailedNonUnify` / `CheckSupportMvcController` decide whether to take the unified path.

## Dependencies
### Internal
- `DataValidation/ValidatorContext.cs`, `DataValidation/Internal/ValidationMetadata.cs`, `FriendlyException/AppFriendlyException`, `UnifyResult/UnifyContext`, `DynamicApiController/` (`IsApiController`, WebSocket helper).
### External
- `Microsoft.AspNetCore.Mvc`, `Microsoft.AspNetCore.Http`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
