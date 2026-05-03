<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Events

## Purpose
EventArgs types raised by the remote-request pipeline. Currently a single payload describing an HTTP request that errored — used by global / per-interface exception interceptors and failure-event subscribers.

## Key Files
| File | Description |
|------|-------------|
| `HttpRequestFaildedEventArgs.cs` | `sealed` `EventArgs` carrying `HttpRequestMessage Request`, `HttpResponseMessage Response` (may be null), and `Exception Exception`. Setters are `internal` — populated only inside the pipeline. |

## For AI Agents

### Working in this directory
- The misspelling `Faild` (should be `Failed`) is intentional — fixing it would be a breaking API change for downstream subscribers. Leave it; document if you want, do not rename.
- Setters are `internal`; subscribers should treat the args as read-only — adding mutable public properties would let handlers corrupt pipeline state.

### Common patterns
- `[SuppressSniffer]` on event-args types so they are not picked up by DI scanning.

## Dependencies
### External
- `System.Net.Http`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
