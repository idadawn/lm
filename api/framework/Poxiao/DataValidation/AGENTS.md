<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataValidation

## Purpose
Poxiao's global data validation feature. Provides a `[DataValidation]` attribute, an enum-backed `ValidationTypes` catalog (regex-driven, e.g. `Numeric`, `IDCard`, `Date`, `Money` …), MVC / Razor Pages action filters that translate model-state failures into `AppFriendlyException` / `BadPageResult`, plus DI extensions to wire it all in (`AddDataValidation` / `AddDataValidation<TValidationMessageTypeProvider>`).

## Key Files
| File | Description |
|------|-------------|
| `ValidatorContext.cs` | Internal helper that flattens `ModelStateDictionary` / `ValidationProblemDetails` / `Dictionary<string,string[]>` into a `ValidationMetadata` (first error property + JSON message). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `DataValidation`, `NonValidation`, `ValidationItemMetadata`, `ValidationMessage(Type)` attributes (see `Attributes/AGENTS.md`). |
| `Enums/` | `ValidationPattern` (`AllOfThem` / `AtLeastOne`) and the regex-tagged `ValidationTypes` enum (see `Enums/AGENTS.md`). |
| `Extensions/` | Public `TryValidate` / `Validate` extensions and the `AddDataValidation` DI extension (see `Extensions/AGENTS.md`). |
| `Filters/` | `DataValidationFilter` (MVC) and `DataValidationPageFilter` (Razor) (see `Filters/AGENTS.md`). |
| `Internal/` | `DataValidationResult`, `ValidationMetadata` DTOs (see `Internal/AGENTS.md`). |
| `Options/` | `ValidationTypeMessageSettingsOptions` for json-driven messages (see `Options/AGENTS.md`). |
| `Providers/` | `IValidationMessageTypeProvider` for plug-in enum-based catalogs (see `Providers/AGENTS.md`). |
| `Validators/` | `DataValidator` static facade (`TryValidateObject` / `TryValidateValue`). |

## For AI Agents

### Working in this directory
- Failure path goes through `AppFriendlyException` (see `FriendlyException/`) — don't throw raw `ValidationException` from new code.
- Localisation is via `Poxiao.Localization.L.Text[message]`; always pass through `L` for user-visible error strings.
- New validation rules are *enum members* with `[Description]` + `[ValidationItemMetadata(regex, defaultMessage)]` — see `Enums/ValidationTypes.cs`.

### Common patterns
- Enum-driven validation catalog reflected at runtime; results compose via `ValidationPattern.AllOfThem`/`AtLeastOne`.

## Dependencies
### Internal
- `FriendlyException/`, `UnifyResult/`, `ConfigurableOptions/`, `DynamicApiController/`, `Localization/`.
### External
- `Microsoft.AspNetCore.Mvc`, `System.ComponentModel.DataAnnotations`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
