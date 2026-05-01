<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Providers

## Purpose
Plug-in contract that lets host applications register additional validation-type catalogs (extra enum types) on top of the built-in `ValidationTypes`. The provider is injected as a singleton via `AddDataValidation<TValidationMessageTypeProvider>` and discovered by the validator at runtime.

## Key Files
| File | Description |
|------|-------------|
| `IValidationMessageTypeProvider.cs` | Single-method contract: `Type[] Definitions { get; }` — returns the validation-type enums (each tagged `[ValidationType]` with members tagged `[ValidationItemMetadata]`) to merge into the catalog. |

## For AI Agents

### Working in this directory
- Implementations should return `Type[]` of enums marked `[ValidationType]`; non-enum types will be ignored by `DataValidator`.
- Singleton lifetime is enforced by `AddDataValidation<TValidationMessageTypeProvider>` (`AddSingleton<IValidationMessageTypeProvider, T>()`); avoid mutable state in implementations.
- Keep this interface intentionally minimal — runtime extension lives in the consumer's enum members, not in extra interface methods.

### Common patterns
- `Type[]` capability provider, paired with `ValidationTypeMessageSettingsOptions` for json-driven message overrides.

## Dependencies
### Internal
- `DataValidation/Attributes/ValidationTypeAttribute.cs`, `DataValidation/Attributes/ValidationItemMetadataAttribute.cs`, consumed by `DataValidation/Validators/DataValidator.cs`.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
