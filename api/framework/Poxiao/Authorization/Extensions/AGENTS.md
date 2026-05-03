<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
策略授权的 DI 注册入口与上下文辅助。`AddAppAuthorization<TAuthorizationHandler>` 一次性注册 `IAuthorizationPolicyProvider`/`IAuthorizationHandler`，可选启用全局 `AuthorizeFilter`；`AuthorizationHandlerContextExtensions` 提供从 `AuthorizationHandlerContext` 读取当前 `HttpContext` 的便捷方法。

## Key Files
| File | Description |
|------|-------------|
| `AuthorizationServiceCollectionExtensions.cs` | `AddAppAuthorization<THandler>(configure, enableGlobalAuthorize)`：注册 `AppAuthorizationPolicyProvider` + 业务 Handler，可启用全局 `AuthorizeFilter` |
| `AuthorizationHandlerContextExtensions.cs` | 从 `AuthorizationHandlerContext` 提取 `HttpContext`、`Endpoint`、`Action` 描述符等 |

## For AI Agents

### Working in this directory
- 业务模块只调用一次 `AddAppAuthorization<MyAppAuthorizeHandler>()`——多次调用会冲突。
- 业务需要全局保护时把 `enableGlobalAuthorize=true`，否则匿名访问需显式 `[AllowAnonymous]`。
- 命名空间 `Microsoft.Extensions.DependencyInjection`，与 ASP.NET Core 习惯保持一致。

### Common patterns
- `services.TryAddSingleton<...>` 防止重复注册。
- 上下文扩展返回 `null` 时由调用方决定降级处理。

## Dependencies
### Internal
- `Authorization/Providers`、`Authorization/Handlers`
### External
- `Microsoft.AspNetCore.Authorization`、`Microsoft.AspNetCore.Mvc.Authorization`、`Microsoft.Extensions.DependencyInjection.Extensions`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
