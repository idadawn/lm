<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
Declarative validation surface for the DataValidation feature. Authoring code annotates DTOs / parameters with `[DataValidation(ValidationPattern, ...types)]`, opts members out with `[NonValidation]`, and authors plug-in regex catalogs with `[ValidationType]` + `[ValidationItemMetadata]` on enum members.

## Key Files
| File | Description |
|------|-------------|
| `DataValidationAttribute.cs` | Subclass of `ValidationAttribute`. Two ctors: `(ValidationPattern, params object[] types)` or `(params object[] types)` defaulting to `AllOfThem`. `IsValid` honours `AllowNullValue` / `AllowEmptyStrings`, calls `value.TryValidate(pattern, types)`, and produces a localised message via `L.Text[errorMessage]`. |
| `NonValidationAttribute.cs` | Marker attribute consulted by `DataValidationFilter` to skip validation on a method or its declaring class. |
| `ValidationItemMetadataAttribute.cs` | Field-level metadata carrying `RegularExpression`, `DefaultErrorMessage`, `RegexOptions` — applied to enum members in `ValidationTypes` to declare the regex and default message. |
| `ValidationMessageAttribute.cs` | Attaches a custom message to a single validation type entry. |
| `ValidationMessageTypeAttribute.cs` | Marks a class/enum as a participating validation message type discovered via `IValidationMessageTypeProvider`. |
| `ValidationTypeAttribute.cs` | Marks an enum (e.g. `ValidationTypes`) as a validation-type catalog. |

## For AI Agents

### Working in this directory
- `DataValidationAttribute` accepts `params object[]` so callers can pass enum members from any catalog — keep it `object`, do not narrow to `ValidationTypes`.
- `[ValidationItemMetadata]` is `AttributeUsage(AttributeTargets.Field)` — it sits on enum members, not classes.
- Localisation lookup `L.Text == null ? errorMessage : L.Text[errorMessage]` guards bootstrap; preserve the null check.

### Common patterns
- `params object[] types` lets composition of multiple validation types per attribute usage.

## Dependencies
### Internal
- `DataValidation/Enums/ValidationPattern.cs`, `DataValidation/Extensions/DataValidationExtensions.cs`, `Localization/L.Text`.
### External
- `System.ComponentModel.DataAnnotations`, `System.Text.RegularExpressions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
