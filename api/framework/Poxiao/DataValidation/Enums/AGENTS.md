<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
The built-in validation catalog. `ValidationPattern` controls how multiple types combine (`AllOfThem` / `AtLeastOne`). `ValidationTypes` is the regex-driven enum the rest of the LIMS uses for inline DTO validation — each member carries a `[Description]` (Chinese label) and `[ValidationItemMetadata(regex, defaultMessage)]`.

## Key Files
| File | Description |
|------|-------------|
| `ValidationPattern.cs` | Two-value enum used by `DataValidationAttribute` and `DataValidator`. `AllOfThem` (默认) requires every type to pass; `AtLeastOne` short-circuits on first success. |
| `ValidationTypes.cs` | `[ValidationType]`-tagged enum. Members include `Numeric`, `PositiveNumber`, `NegativeNumber`, `Integer`, `Money`, `Date`, `Time`, `IDCard`, plus the rest of the standard catalog. Each carries a Chinese `[Description]` and an English `DefaultErrorMessage` regex pair. |

## For AI Agents

### Working in this directory
- Keep `[Description]` Chinese (matches the LIMS UI) and `DefaultErrorMessage` English (used as the localisation lookup key in `L.Text`).
- Adding a new built-in type means appending an enum member with both attributes — do *not* renumber existing members; they may be persisted in JSON or referenced by ordinal.
- Project-specific catalogs should live in *their* feature folder, declared as their own enum and surfaced via `IValidationMessageTypeProvider.Definitions`.

### Common patterns
- Regex literals are `@"..."` raw strings; the matching options pass through `ValidationItemMetadata.RegexOptions` (default `None`).

## Dependencies
### Internal
- `DataValidation/Attributes/ValidationItemMetadataAttribute.cs`, `DataValidation/Attributes/ValidationTypeAttribute.cs`.
### External
- `System.ComponentModel`, `System.Text.RegularExpressions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
