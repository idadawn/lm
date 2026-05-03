<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
The reflection/expression-tree engine that turns the declarative `[OptionsBuilder]` attribute + `IConfigureOptionsBuilder<...>` interfaces into actual `Microsoft.Extensions.Options.OptionsBuilder<T>` calls (`Bind`, `Configure`, `PostConfigure`, `Validate`, `ValidateDataAnnotations`).

## Key Files
| File | Description |
|------|-------------|
| `OptionsBuilderExtensions.cs` | `ConfigureBuilder<TOptions>` / `ConfigureBuilders<TOptions>` / `ConfigureDefaults<TOptions>`. `ConfigureDefaults` reads `[OptionsBuilder]`, derives the section key (defaults to type-name minus `Options` suffix), binds the section, and registers DataAnnotations / `IValidateOptions<T>` validators. `ConfigureBuilder` enumerates `IOptionsBuilderDependency<TOptions>`-derived interfaces, builds an `Action<...>`/`Func<..., bool>` delegate via `Expression.Lambda`, and dynamically invokes the matching `Configure`/`PostConfigure`/`Validate` overload. |

## For AI Agents

### Working in this directory
- `CreateDelegate(inputTypes, outputType)` constructs `System.Action`<…> / `System.Func`<…> types by name (`$"{baseDelegateType.FullName}` `{n}"`); changing arity rules here affects every options POCO. Touch with care.
- `ConfigureDefaults` calls `optionsBuilder.Bind(section, ...)`. The `BindNonPublicProperties` and `ErrorOnUnknownConfiguration` knobs come straight from the `[OptionsBuilder]` attribute — propagate any new flag from the attribute through here.

### Common patterns
- Expression-tree-based dynamic dispatch instead of `MethodInfo.Invoke` to preserve the delegate-type signature `OptionsBuilder<T>.Configure(Action<TDep1, TDep2...>)` expects.

## Dependencies
### Internal
- `Poxiao.Options.Attributes`, `Poxiao.Options.Constants`, `Poxiao.Options.Dependencies`, `Poxiao.Extensions` (string/array helpers).

### External
- `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Options`, `Microsoft.Extensions.DependencyInjection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
