<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extras.ObjectMapper.Mapster

## Purpose
Object-mapping adapter that plugs Mapster (`Mapster` 7.4.0 + `Mapster.DependencyInjection` 1.0.1) into the Poxiao DI container. Used across modules to map between `Entity` and `Dto` types under `api/src/modularity/*/Dto`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `AddObjectMapper` DI helper (see `Extensions/AGENTS.md`). |

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Extras.ObjectMapper.Mapster.csproj` | net10.0 packable library; pulls Mapster + Mapster.DependencyInjection. |

## For AI Agents

### Working in this directory
- Mapper registrations live in module-level `IRegister` classes; this assembly only provides the scaffolding.
- The default name-matching strategy registered here is `Flexible` then `IgnoreCase` (last-write-wins); align module mappings to that.
- Keep this assembly focused on Mapster-only adapters — avoid adding AutoMapper or other mapper libs.

### Common patterns
- `services.AddSingleton(config)` for `TypeAdapterConfig.GlobalSettings` so all scopes share compiled mappings.
- `services.AddScoped<IMapper, ServiceMapper>()` to support DI inside mapping projections.

## Dependencies
### External
- `Mapster` 7.4.0, `Mapster.DependencyInjection` 1.0.1, `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.1.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
