<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
DI 与中间件注册扩展。为 `IServiceCollection` / `IMvcBuilder` / `IApplicationBuilder` 添加 UnifyResult 相关的注入入口：开启规范化、注册自定义 Provider、绑定具名 JSON 序列化设置、挂载短路状态码中间件。

## Key Files
| File | Description |
|------|-------------|
| `UnifyResultServiceCollectionExtensions.cs` | `AddUnifyResult` / `AddUnifyResult<TProvider>` / `AddUnifyProvider<T>(name)` / `AddUnifyJsonOptions(name, settings)`，在内部翻转 `UnifyContext.EnabledUnifyHandler = true` 并装配 `SucceededUnifyResultFilter`。 |
| `UnifyResultMiddlewareExtensions.cs` | `UseUnifyResultStatusCodes()` — 注册 `UnifyResultStatusCodesMiddleware`，处理 ≥400 短路状态码。 |

## For AI Agents

### Working in this directory
- 启动顺序：`AddUnifyResult` 必须先于其它依赖 `UnifyContext.EnabledUnifyHandler` 的代码（如 Swagger 装饰器）。
- 中间件 `UseUnifyResultStatusCodes` 应放在异常处理之后、`UseAuthentication` 之前，否则 401/403 拦截顺序错乱。
- 默认实现注册的是 `RESTfulResultProvider`；要替换全局默认，调 `AddUnifyResult<MyProvider>()`，不要 `Replace<IUnifyResultProvider>`（用的是命名 Singleton）。

### Common patterns
- `services.TryAddSingleton(providerType, providerType)` 注册自身类型，便于按 `UnifyMetadata.ProviderType` 解析。
- `UnifyContext.UnifyProviders.AddOrUpdate` 以 providerName 为键存储元数据。

## Dependencies
### Internal
- `UnifyContext`、`UnifyMetadata`、`SucceededUnifyResultFilter`、`UnifyResultStatusCodesMiddleware`。
### External
- `Microsoft.Extensions.DependencyInjection.Extensions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
