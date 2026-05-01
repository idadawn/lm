<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DependencyInjection

## Purpose
Convention-based DI module for the Poxiao framework. Scans the application for classes implementing `ITransient` / `IScoped` / `ISingleton`, registers them automatically (with optional named/proxy/external definitions from `appsettings.json`), and provides scope helpers plus a named-service resolver. Backbone of how every `Poxiao.*` module wires its services without explicit `services.Add...` calls.

## Key Files
| File | Description |
|------|-------------|
| `Scoped.cs` | `Scoped.Create` / `Scoped.CreateAsync` static helpers to run code inside a fresh `IServiceScope` (used by background jobs/event handlers). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[Injection]`, `[SuppressProxy]`, `[SuppressSniffer]` markers (see `Attributes/AGENTS.md`). |
| `Dependencies/` | Lifetime marker interfaces `IScoped` / `ISingleton` / `ITransient` (see `Dependencies/AGENTS.md`). |
| `Enums/` | Injection action / pattern / register-type enums (see `Enums/AGENTS.md`). |
| `Extensions/` | `AddDependencyInjection` service-collection extensions and proxy registration (see `Extensions/AGENTS.md`). |
| `Internal/` | `ExternalService` config model for assembly-string-based registration (see `Internal/AGENTS.md`). |
| `Options/` | `DependencyInjectionSettingsOptions` bound from configuration (see `Options/AGENTS.md`). |
| `Providers/` | `INamedServiceProvider<T>` and default implementation (see `Providers/AGENTS.md`). |

## For AI Agents

### Working in this directory
- New services in business modules should implement `ITransient` / `IScoped` / `ISingleton` rather than being added manually in `Program.cs`.
- Use `[Injection(InjectionPatterns.SelfWithFirstInterface)]` to control how the registration shape differs from the default.
- For multi-implementation scenarios resolve via `INamedServiceProvider<TService>`, not `IEnumerable<TService>`.

### Common patterns
- Lifetime marker interfaces (no methods) drive registration — keep them empty.
- Configuration-driven external registration via `DependencyInjectionSettingsOptions.Definitions`.

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions`, `Poxiao.Reflection`, `Poxiao.DynamicApiController` (proxy-aware registration).
### External
- `Microsoft.Extensions.DependencyInjection.Extensions`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
