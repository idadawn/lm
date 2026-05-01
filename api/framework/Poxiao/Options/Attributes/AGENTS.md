<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
Attributes that drive the `Poxiao.Options` binding pipeline. They mark options POCOs with their configuration section, attach a failure message to validate methods, and (internally) map builder interfaces to the corresponding `OptionsBuilder<T>` method names.

## Key Files
| File | Description |
|------|-------------|
| `OptionsBuilderAttribute.cs` | `[OptionsBuilder]` on a class — sets `SectionKey`, plus flags `ErrorOnUnknownConfiguration`, `BindNonPublicProperties`, `ValidateDataAnnotations`, and an array of `IValidateOptions<T>` implementor types. |
| `OptionsBuilderMethodMapAttribute.cs` | Internal attribute on builder dependency interfaces (`IConfigureOptionsBuilder<...>`, etc.) — records the target `OptionsBuilder<T>` method name and whether it returns void (used by reflection in `OptionsBuilderExtensions.InvokeMapMethod`). |
| `FailureMessageAttribute.cs` | `[FailureMessage("...")]` on a `Validate` method — emitted as the second argument to `OptionsBuilder<T>.Validate(Func, string)`. |

## For AI Agents

### Working in this directory
- `[OptionsBuilder]` is the **only** public-facing attribute users need on options POCOs. The other two are framework plumbing — `OptionsBuilderMethodMapAttribute` is `internal` for a reason; keep it that way.
- Options classes do **not** declare `[OptionsBuilder]` if the conventional `XxxOptions` → `Xxx` section-key derivation suffices.

### Common patterns
- `[SuppressSniffer]` + `AttributeUsage(..., AllowMultiple = false)` for single-use type-level configuration.

## Dependencies
### Internal
- Consumed by `Options/Extensions/OptionsBuilderExtensions.cs`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
