<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# guard

## Purpose
Vue Router navigation guards composed by `setupRouterGuard(router)`. Implements the auth/permission flow: token check, login redirect, dynamic route injection from backend menu, 404 redirect, and special-case workflow detail token swap.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Top-level `setupRouterGuard` that wires `createPageGuard`, `createScrollGuard`, `createMessageGuard`, `createProgressGuard`, etc. |
| `permissionGuard.ts` | Token-aware guard — handles `whitePathList` (login/SSO/short-link), session-expired, dynamic `addRoute()` from `permissionStore.buildRoutesAction()`, and 404 fallback. |
| `stateGuard.ts` | Resets pinia stores + router on logout transitions. |
| `paramMenuGuard.ts` | Path-parameter menu activation helper. |

## For AI Agents

### Working in this directory
- `whitePathList` only contains public entry pages (login, SSO callback, short-link form). Adding to it bypasses auth — be deliberate.
- Workflow detail accepts an alternate token via `?token=` to allow email-link sign-in; do not remove without checking workflow flow.
- Dynamic routes are added once per session (`isDynamicAddedRoute`); call `resetRouter()` if you need to rebuild.

### Common patterns
- Guards read the user/permission store via `*WithOut` accessors so they work outside setup() context.
- Always `next(...)` exactly once per branch.

## Dependencies
### Internal
- `/@/store/modules/user`, `/@/store/modules/permission`, `/@/router/routes/basic`, `/@/enums/pageEnum`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
