<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Providers

## Purpose
Named-service resolution: lets a consumer with multiple registrations of the same service interface (each tagged with `[Injection(Named = "x")]`) pick a specific implementation by name at runtime. Used heavily across modules where one logical contract has driver-specific implementations (e.g. file storage providers, IM channels, codegen targets).

## Key Files
| File | Description |
|------|-------------|
| `INamedServiceProvider.cs` | Public contract: `GetService(name)`, `GetService<ILifetime>(name)`, `GetRequiredService(name)` overloads. |
| `NamedServiceProvider.cs` | Internal implementation that scans all registered `IEnumerable<TService>` and matches by class name / `[Injection].Named`. |

## For AI Agents

### Working in this directory
- Inject `INamedServiceProvider<TService>` (open-generic, registered as transient in `../Extensions/`) — never construct `NamedServiceProvider<T>` directly.
- The `ILifetime` generic parameter is constrained to `IPrivateDependency`; passing a non-marker fails at compile time.
- Name resolution falls back to the implementation type name when `[Injection].Named` is empty.

### Common patterns
- Open-generic registration: `services.AddTransient(typeof(INamedServiceProvider<>), typeof(NamedServiceProvider<>))` in `AddDependencyInjection`.

## Dependencies
### Internal
- `../Dependencies/IPrivateDependency` and lifetime markers.
### External
- `Microsoft.Extensions.DependencyInjection`, `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
