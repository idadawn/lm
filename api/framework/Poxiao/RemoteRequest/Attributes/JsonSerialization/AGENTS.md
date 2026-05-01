<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# JsonSerialization

## Purpose
Per-interface/method JSON serialization configuration for declarative HTTP calls. Lets a remote-request interface plug in a custom serializer (e.g. switch from `System.Text.Json` to Newtonsoft, or to a project-specific provider that handles snake_case / Chinese-locale dates) and pass `JsonSerializerOptions`-style state through the request pipeline.

## Key Files
| File | Description |
|------|-------------|
| `JsonSerializationAttribute.cs` | `[JsonSerialization(typeof(MyJsonProvider))]` on interface/method — `ProviderType` must implement the framework's `IJsonSerializerProvider` (in `Poxiao.JsonSerialization`). |
| `JsonSerializerOptionsAttribute.cs` | Companion attribute carrying serializer-options metadata; resolved by the JSON provider at call time. |

## For AI Agents

### Working in this directory
- The `ProviderType` is **not** verified at attribute-declaration time. The runtime resolution happens inside `HttpRequestPart` — keep that contract: provider must be DI-resolvable from `IServiceProvider` and implement the `IJsonSerializerProvider` interface in `Poxiao.JsonSerialization`.
- Apply at the most specific scope required: method-level `[JsonSerialization]` overrides interface-level.

## Dependencies
### Internal
- `Poxiao.JsonSerialization` (provider interface).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
