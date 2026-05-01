<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# http

## Purpose
HTTP layer for the web app. Houses the project's only HTTP transport (axios under `axios/`); barrel re-exports `defHttp` so call sites can `import { defHttp } from '/@/utils/http/axios'`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `axios/` | VAxios wrapper, transforms, retry, cancel, status checker, and the `defHttp` instance (see `axios/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Add new HTTP transports here if needed (e.g. WebSocket / EventSource) but keep call-site stability — the bulk of the app imports through `axios/` directly.
- Don't bypass `defHttp` for fetch-based requests in the SSE NLQ-agent client (`/@/api/nlqAgent.ts`); that is intentionally direct because of streaming semantics.

## Dependencies
### Internal
- `axios/` is the live implementation.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
