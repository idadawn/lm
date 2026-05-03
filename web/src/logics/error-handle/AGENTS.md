<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# error-handle

## Purpose
Global error capture pipeline. Installs Vue `app.config.errorHandler`, `window.onerror`, unhandled-promise, resource-error, and (optionally) script-error listeners, then forwards normalized payloads to the `errorLog` Pinia store for the in-app error log table.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Implements stack normalization (`processStackMsg`), component-name resolution, and `setupErrorHandle(app)`; gated by `projectSetting.useErrorHandle`. |

## For AI Agents

### Working in this directory
- Do not log PII — captured stacks are forwarded to the user-visible error log view.
- Stack length is capped (Error.stackTraceLimit = 10) — keep that in sync if extended.
- Resource error listeners are passive `capture: true` — leave that as-is to avoid double-fire.

### Common patterns
- Pushes via `useErrorLogStoreWithOut().addErrorLogInfo(...)` so it works pre-pinia-app-mount.
- Categorizes errors with `ErrorTypeEnum` (vue / script / resource / promise / api).

## Dependencies
### Internal
- `/@/store/modules/errorLog`, `/@/enums/exceptionEnum`, `/@/settings/projectSetting`.
### External
- `vue` (`App` type).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
