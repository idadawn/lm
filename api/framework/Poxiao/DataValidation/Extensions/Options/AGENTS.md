<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
DI-time configuration object for `AddDataValidation`. Callers pass an `Action<DataValidationOptions>` to opt out of global validation, change the C#8 nullable-reference behaviour, or restore the built-in MVC `ModelStateInvalidFilter` / client-error mapping.

## Key Files
| File | Description |
|------|-------------|
| `DataValidationOptions.cs` | Sealed POCO. `GlobalEnabled` (default `true`), `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes` (default `true`), `SuppressModelStateInvalidFilter` (default `true`, only honoured when `GlobalEnabled`), `SuppressMapClientErrors` (default `false`). |

## For AI Agents

### Working in this directory
- This class is **not** an `IConfigurableOptions` — it's a plain configurator passed inline to `AddDataValidation(...)`. Don't move it under `DataValidation/Options/`, where the JSON-bound options live.
- Defaults are the contract: flipping any of them changes whether MVC double-handles validation. If you do, audit every `[NonValidation]` and every controller that catches `ValidationException`.

### Common patterns
- Plain DTO with public setters; configured via `Action<TOptions>` rather than DI.

## Dependencies
### Internal
- Read by `DataValidation/Extensions/DataValidationServiceCollectionExtensions.cs`.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
