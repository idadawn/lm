<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
DTOs that move data between the validation extensions, the MVC filter, and the unified-result provider. Not consumed directly by feature code — the public surface is `DataValidationResult` (returned by `TryValidate`) plus `ValidationMetadata` (carried in `HttpContext.Items` for downstream serialisation).

## Key Files
| File | Description |
|------|-------------|
| `DataValidationResult.cs` | `[SuppressSniffer]` sealed POCO. `IsValid`, `ICollection<ValidationResult> ValidationResults`, `MemberOrValue` (the property name or value being validated). Returned by `DataValidator.TryValidate*`. |
| `ValidationMetadata.cs` | Sealed POCO produced by `ValidatorContext.GetValidationMetadata`. Carries `ValidationResult` (dict / string), serialised JSON `Message`, `ModelStateDictionary ModelState`, `ErrorCode` / `OriginErrorCode` / `StatusCode`, `FirstErrorProperty`, `FirstErrorMessage`, optional `Data`. Setters are `internal` — only `ValidatorContext` / `DataValidationFilter` populate it. |

## For AI Agents

### Working in this directory
- All setters that need framework-only mutation are `internal set;` — preserve so feature code cannot fabricate a fake "passed" result.
- These types appear on the public `TryValidate` return type, so renaming `ValidationResults`/`MemberOrValue` is a breaking change.

### Common patterns
- Result + metadata DTO pair: `DataValidationResult` for callers, `ValidationMetadata` for filters/unify provider.

## Dependencies
### Internal
- `DataValidation/Validators/DataValidator.cs` (writes `DataValidationResult`); `DataValidation/ValidatorContext.cs` (writes `ValidationMetadata`).
### External
- `System.ComponentModel.DataAnnotations`, `Microsoft.AspNetCore.Mvc.ModelBinding`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
