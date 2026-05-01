<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
DI registration helpers that wire the Mapster-backed object mapper into the host. Contains a single static class that locates the `Poxiao.Extras.ObjectMapper.Mapster` extras assembly at startup and delegates the real `AddObjectMapper` call into it.

## Key Files
| File | Description |
|------|-------------|
| `ObjectMapperServiceCollectionExtensions.cs` | `IServiceCollection.AddObjectMapper()` — reflection-driven invocation of the extras package's identical method, passing `App.Assemblies.ToArray()` so Mapster can scan all loaded modules for `IRegister`/profile types. |

## For AI Agents

### Working in this directory
- The constant `ASSEMBLY_NAME = "Poxiao.Extras.ObjectMapper.Mapster"` must match the actual extras package name. Do not rename without coordinating with that package.
- The extension is a no-op when the extras package is not loaded — keep that "graceful fallback" semantic; do not throw.

### Common patterns
- `[SuppressSniffer]` on host-targeted extension classes (Poxiao convention to hide from auto-DI scanning).
- Lookup helpers `App.Assemblies.FirstOrDefault(...)` + `Reflect.GetType` + `MethodInfo.Invoke` — the standard "optional-feature plug-in" pattern across the framework.

## Dependencies
### Internal
- `Poxiao.Reflection` (`Reflect`)
- `Poxiao` host (`App.Assemblies`)

### External
- `Microsoft.Extensions.DependencyInjection.Abstractions`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
