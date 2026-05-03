<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Validators

## Purpose
Houses the static `DataValidator` engine used by Poxiao's data-validation subsystem. Discovers all `[ValidationItemMetadata]`-decorated regex/rule definitions across loaded assemblies, caches them with concurrent dictionaries, and exposes `TryValidateObject` / `TryValidateValue` style helpers consumed by MVC filters and service-layer validation.

## Key Files
| File | Description |
|------|-------------|
| `DataValidator.cs` | Static validator: scans validation types, builds metadata cache, performs object/property/value validation honoring `[NonValidation]` opt-outs. |

## For AI Agents

### Working in this directory
- This is framework code; treat as stable infrastructure. Do not add laboratory/business logic here.
- New built-in validators belong with the rest of `Poxiao/DataValidation/` (parent dir), not under `Validators/`. This dir is the engine entry point.
- Preserve the `[SuppressSniffer]` attribute and the static-constructor caching pattern when editing.

### Common patterns
- Reflection-driven discovery via `App.EffectiveTypes` (Furion-style global type cache).
- `ConcurrentDictionary` caches keyed on validation enum values for thread-safe lookup.
- Returns `DataValidationResult` rather than throwing, so callers can branch on success.

## Dependencies
### Internal
- `Poxiao.Extensions` (reflection helpers), `Poxiao.Templates.Extensions` (string templating for messages), the wider `Poxiao.DataValidation` namespace (attributes, result type, message resources).
### External
- `Microsoft.AspNetCore.Mvc`, `System.ComponentModel.DataAnnotations`, `System.Text.RegularExpressions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
