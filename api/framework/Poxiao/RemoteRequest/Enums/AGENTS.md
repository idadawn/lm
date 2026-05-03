<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
Enumerations used by `RemoteRequest`'s declarative pipeline — currently the interceptor-stage enum that classifies where a user-supplied delegate plugs into the request lifecycle.

## Key Files
| File | Description |
|------|-------------|
| `InterceptorTypes.cs` | `Initiate` (HttpClient creation), `Client` (HttpClient ready), `Request` (HttpRequestMessage), `Response` (HttpResponseMessage), `Exception` (failure). Each member has `[Description]` for UI / logging surfaces. |

## For AI Agents

### Working in this directory
- Enum values map 1:1 to interceptor invocation points inside `HttpDispatchProxy.BuildHttpRequestPart` and `HttpRequestPart.SendAsync`. Adding a new value requires a corresponding hook in those code paths.
- The `[Description]` text is in Chinese to match the rest of the framework's user-facing diagnostics — keep this convention.

## Dependencies
### External
- `System.ComponentModel.Description` for the localized member names.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
