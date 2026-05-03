<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Handlers

## Purpose
ASP.NET Core authorization handlers specific to the API host. Currently contains the JWT-driven `AppAuthorizeHandler` implementation that bridges Furion's auth pipeline with the project's RBAC permission convention.

## Key Files
| File | Description |
|------|-------------|
| `JwtHandler.cs` | Inherits `Poxiao.Authorization.AppAuthorizeHandler`. Synchronizes `httpContext.User` with `AuthorizationHandlerContext`, fast-paths administrators (`ClaimConst.CLAINMADMINISTRATOR == AccountType.Administrator`) and computes the `routeName` (path with leading `/api` stripped, `/` → `:`) used to look up button/API permissions. The actual permission check is currently stubbed to `return true` — the SysMenu lookup is commented out. |

## For AI Agents

### Working in this directory
- `JwtHandler` is registered via `services.AddJwt<JwtHandler>(enableGlobalAuthorize: true, ...)` in `Startup.cs` — do not also call `services.AddAuthorization` manually.
- The default route allow-list (`oauth:CurrentUser`) lives inline in `CheckAuthorzieAsync`; add new public endpoints there or via `[AllowAnonymous]` on the controller.
- Re-enable the real permission check by injecting `ISysMenuService` and uncommenting the `permissionList.Contains(routeName)` branch — coordinate with the System module RBAC owners before doing so.
- Token can arrive via `Authorization` header **or** `?token=` query parameter (see `OnMessageReceived` in `Startup.cs`). WebSocket clients rely on the query form.

### Common patterns
- Override `HandleAsync` only to bridge user context, then delegate to `AuthorizeHandleAsync` (Furion's pipeline entrypoint).
- Override `PipelineAsync` to return the boolean authorization result.

## Dependencies
### Internal
- `framework/Poxiao` core (`AppAuthorizeHandler`, `ClaimConst`, `AccountType`).

### External
- `Microsoft.AspNetCore.Authorization`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
