<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Middlewares

## Purpose
针对 ≥400 的短路状态码（如 401 未授权、403 禁止、500 内部错误）在请求管道末端进行规范化输出。MVC Filter 处理不到这些响应（无 Action 返回值），因此需要中间件兜底，把响应写为 `RESTfulResult` JSON。

## Key Files
| File | Description |
|------|-------------|
| `UnifyResultStatusCodesMiddleware.cs` | `await _next` 之后检查 `Response.StatusCode`；跳过 WebSocket、<400、404；401 + 双 Token Header 时改写 403；通过 `UnifyContext.CheckStatusCodeNonUnify` 解析 Provider 后调用 `OnResponseStatusCodes`。 |

## For AI Agents

### Working in this directory
- 中间件不处理 404（避免静态资源/路由未命中也被强行包装）；如需处理在 `IUnifyResultProvider.OnResponseStatusCodes` 内手动加。
- `Response.HasStarted == true` 时禁止再次写入，否则 Kestrel 抛 `InvalidOperationException`。
- 注册位置：`UseUnifyResultStatusCodes()` 应在 `UseRouting/UseAuthentication/UseAuthorization` 之后、终结点中间件之前。

### Common patterns
- `endpointFeature.Endpoint.Metadata.GetMetadata<NonUnifyAttribute>()` 用于跳过显式声明 NonUnify 的端点。

## Dependencies
### Internal
- `UnifyContext`、`UnifyResultSettingsOptions`、`IUnifyResultProvider`。
### External
- `Microsoft.AspNetCore.Http`、`Microsoft.Extensions.DependencyInjection`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
