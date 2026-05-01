<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Parameters

## Purpose
Method-parameter-targeting attributes for declarative HTTP interfaces. Mark which actual call-site argument should become the request body, query string, or other request component.

## Key Files
| File | Description |
|------|-------------|
| `ParameterBaseAttribute.cs` | Empty base class with `AttributeUsage(AttributeTargets.Parameter)` — discovery marker so `HttpRequestPart` argument scanning can pick up subclasses with a single `IsDefined` check. |
| `BodyAttribute.cs` | `[Body]` / `[Body("application/x-www-form-urlencoded")]` / `[Body("application/json","UTF-8")]` — flags a parameter as the HTTP body and overrides content-type / encoding. Defaults: `application/json`, `UTF-8`. |
| `QueryStringAttribute.cs` | `[QueryString]` — flags a parameter (typically a primitive or POCO) as a query-string source. |

## For AI Agents

### Working in this directory
- Always derive from `ParameterBaseAttribute` for any new parameter-binding attribute (e.g. a future `[Form]`, `[Path]`) — the proxy iterates parameters and checks for this base type.
- A parameter without any `ParameterBaseAttribute` in the framework's heuristic typically becomes a query argument when the verb is `GET`/`HEAD`/`DELETE` or part of the body otherwise; preserve that convention if you add new attributes.

## Dependencies
### Internal
- Consumed by `RemoteRequest/Internal/HttpRequestPart.cs` parameter scanning.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
