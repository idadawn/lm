<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ObjectMapper

## Purpose
Object-mapping abstraction layer for the Poxiao framework. The actual mapping engine ships in a separate `Poxiao.Extras.ObjectMapper.Mapster` assembly (Mapster-based) that this directory dynamically discovers and forwards to. This keeps Mapster as an optional dependency.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `IServiceCollection` registration helpers (see `Extensions/AGENTS.md`) |

## For AI Agents

### Working in this directory
- This namespace is intentionally thin. Do **not** add concrete mappers here — they belong in the `Poxiao.Extras.ObjectMapper.Mapster` extras package.
- The `AddObjectMapper` extension probes `App.Assemblies` for the extras assembly via reflection (`Reflect.GetType`); if the extras package is not referenced the call is a no-op. Preserve that fallback when modifying.

### Common patterns
- Reflection-based late binding: a fixed assembly name constant (`Poxiao.Extras.ObjectMapper.Mapster`) is resolved at startup, then `AddObjectMapper(IServiceCollection, Assembly[])` is invoked dynamically with `App.Assemblies`.

## Dependencies
### Internal
- `Poxiao.Reflection` (uses `Reflect.GetType` for assembly probing)
- `Poxiao.App` (host context — `App.Assemblies`)

### External
- Mapster (only if the extras package is installed at runtime)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
