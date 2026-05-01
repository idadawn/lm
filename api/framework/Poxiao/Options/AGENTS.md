<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
Poxiao 框架的“选项构建器”基础设施 — a typed wrapper over `Microsoft.Extensions.Options`. Lets POCOs declare `[OptionsBuilder("Section")]`, optionally implement `IConfigureOptionsBuilder<T>` / `IPostConfigureOptionsBuilder<T>` / `IValidateOptionsBuilder<T>`, and have configuration binding, post-configuration and validation wired up automatically against a configuration section (defaulting to the type name minus the `Options` suffix).

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[OptionsBuilder]`, `[FailureMessage]`, internal method-map attribute (see `Attributes/AGENTS.md`) |
| `Constants/` | Internal constants — section-key suffix trim rules (see `Constants/AGENTS.md`) |
| `Dependencies/` | `IConfigureOptionsBuilder<T,...>` / `IPostConfigureOptionsBuilder<...>` / `IValidateOptionsBuilder<...>` interfaces (see `Dependencies/AGENTS.md`) |
| `Extensions/` | `OptionsBuilder<T>` extension methods that drive the binding/validation pipeline (see `Extensions/AGENTS.md`) |

## For AI Agents

### Working in this directory
- New options POCOs should be named `XxxOptions` so the auto-derived section key drops the `Options` suffix (see `Constants/Constants.cs`). Override with `[OptionsBuilder("CustomKey")]` only if the section name diverges.
- For `Configure`/`PostConfigure`/`Validate` with DI dependencies, implement the matching `I*OptionsBuilder<TOptions, TDep1, ...>` interface — the `OptionsBuilderExtensions.ConfigureBuilder` reflection pipeline maps method signatures via `[OptionsBuilderMethodMap]`.
- Use `[FailureMessage("...")]` on a `Validate` method to surface a user-friendly error string.

### Common patterns
- Reflection over generic interfaces + `Expression.Lambda` codegen to call the underlying `OptionsBuilder<TOptions>.Configure/PostConfigure/Validate` overloads with the right `Action<...>` / `Func<...>` delegate signature.

## Dependencies
### Internal
- `Poxiao.Extensions` (string/array helpers like `ClearStringAffixes`, `IsEmpty`)

### External
- `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Options`, `Microsoft.Extensions.DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
