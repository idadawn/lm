<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dependencies

## Purpose
Marker / strategy interfaces that an options POCO can implement to participate in the `Poxiao.Options` configuration pipeline without touching the host startup code. Each variant carries an `[OptionsBuilderMethodMap]` attribute that points the reflection driver at the correct `OptionsBuilder<TOptions>.{Configure|PostConfigure|Validate}` overload.

## Key Files
| File | Description |
|------|-------------|
| `IOptionsBuilderDependency.cs` | Marker base interface `IOptionsBuilderDependency<TOptions>` that every other interface inherits — used by `ConfigureBuilder` to enumerate participating interfaces via `Type.GetInterfaces()`. |
| `IConfigureOptionsBuilder.cs` | `IConfigureOptionsBuilder<TOptions[, TDep1..TDep5]>` — `void Configure(TOptions, TDep1...)`; mapped to `OptionsBuilder.Configure`. Provides 0-5 dependency overloads. |
| `IPostConfigureOptionsBuilder.cs` | Sibling interface for the `PostConfigure` lifecycle stage. |
| `IValidateOptionsBuilder.cs` | Validation contract — methods return `bool` and the framework reads `[FailureMessage]` for the user-visible error text. |

## For AI Agents

### Working in this directory
- Implementing `IConfigureOptionsBuilder<TOptions>` directly on the options class itself is the idiomatic pattern; you may also place the implementation on a separate type and pass the type via `ConfigureBuilder(typeof(MyConfigurer))`.
- Stay within 0–5 `TDep` overloads — adding more requires matching changes in `OptionsBuilderExtensions.CreateDelegate` (which constructs `Action`/`Func` types by arity from the runtime type system).

### Common patterns
- Generic interface arity-overloading (TOptions, TDep1..TDep5) mirrors the upstream `Microsoft.Extensions.Options` API; method signatures must match exactly (name + parameter count) for `InvokeMapMethod` to bind.

## Dependencies
### Internal
- `Options/Attributes/OptionsBuilderMethodMapAttribute.cs`

### External
- `Microsoft.Extensions.Options` (`OptionsBuilder<T>`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
