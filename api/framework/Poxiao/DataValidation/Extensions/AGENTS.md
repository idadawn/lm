<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Public surface of the DataValidation feature. `DataValidationExtensions` exposes fluent `TryValidate` / `Validate` extensions on any `object` (object-graph, value+attributes, value+types, value+regex). `DataValidationServiceCollectionExtensions` wires the global filter pipeline (`DataValidationFilter`, `DataValidationPageFilter`) and binds `ValidationTypeMessageSettingsOptions`.

## Key Files
| File | Description |
|------|-------------|
| `DataValidationExtensions.cs` | Object extensions. `TryValidate` overloads return a `DataValidationResult`; `Validate` overloads call `ThrowValidateFailedModel()` which throws `AppFriendlyException(StatusCode=400, ValidationException=true, ErrorMessage=dict)`. Includes a regex overload `TryValidate(value, regexPattern, regexOptions)` returning `bool`. |
| `DataValidationServiceCollectionExtensions.cs` | DI extensions. `AddDataValidation(configure)` / `AddDataValidation<TProvider>(configure)`. Configures `ApiBehaviorOptions` (`SuppressMapClientErrors`, `SuppressModelStateInvalidFilter`), wires `AddMvcFilter<DataValidationFilter>` + `AddMvcFilter<DataValidationPageFilter>`, and binds the message provider singleton. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Options/` | `DataValidationOptions` for the DI configurator (see `Options/AGENTS.md`). |

## For AI Agents

### Working in this directory
- The public `TryValidate` family is *the* call surface — feature modules should not new up `DataValidator` directly.
- Failure throw goes through `AppFriendlyException`; `StatusCode = 400` and `ValidationException = true` are required so `DataValidationFilter.CallUnHandleResult` can recognise it.
- `AddDataValidation` flips `SuppressModelStateInvalidFilter = true` by default — if you turn that off, expect duplicate 400 responses from MVC.

### Common patterns
- Extension-method facade hiding `DataValidator.TryValidateValue` / `DataValidator.TryValidateObject`.
- DI registration for both MVC controllers and Razor Pages.

## Dependencies
### Internal
- `DataValidation/Validators/DataValidator.cs`, `FriendlyException/AppFriendlyException`, `DataValidation/Filters/`, `ConfigurableOptions/`.
### External
- `Microsoft.AspNetCore.Mvc`, `Microsoft.AspNetCore.Http`, `System.ComponentModel.DataAnnotations`, `System.Text.RegularExpressions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
