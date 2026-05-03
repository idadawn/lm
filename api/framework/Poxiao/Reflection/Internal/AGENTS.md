<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Internal-visibility helper types used inside `Poxiao.Reflection`. Currently a single value-object that bundles a `ParameterInfo` together with the runtime-supplied name and value at the call-site — used by proxy machinery when collecting argument metadata for downstream code (e.g. building HTTP query strings from `[QueryString]`-annotated parameters).

## Key Files
| File | Description |
|------|-------------|
| `MethodParameterInfo.cs` | `internal class` with `ParameterInfo Parameter`, `string Name`, `object Value`. Plain DTO; no behaviour. |

## For AI Agents

### Working in this directory
- Keep these types `internal` — they are an implementation detail of the proxy code paths. Public APIs should expose richer wrappers (e.g. `Invocation`).
- Do not rename properties without updating proxy consumers in `Poxiao.RemoteRequest`.

## Dependencies
### External
- `System.Reflection`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
