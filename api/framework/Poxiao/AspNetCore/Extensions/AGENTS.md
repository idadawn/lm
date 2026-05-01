<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
ASP.NET Core 抽象的扩展方法集合：`HttpContext` 元数据/Action 描述符获取、Swagger 自动登入登出；`IHost` 启动辅助；MVC 过滤器统一注册；`ModelBindingContext` 工具方法。这些方法被框架其他子系统（`Authorization`、`UnifyResult`、`SpecificationDocument`）和业务模块共享。

## Key Files
| File | Description |
|------|-------------|
| `HttpContextExtensions.cs` | `GetMetadata<TAttribute>`、`GetControllerActionDescriptor`、`SigninToSwagger`/`SignoutToSwagger`，封装 Endpoint/Action 元数据访问 |
| `AspNetCoreBuilderServiceCollectionExtensions.cs` | `AddMvcFilter<TFilter>`：泛型注册 MVC 过滤器并配置 `MvcOptions` |
| `IHostExtensions.cs` | `IHost` 启动/资源解析辅助 |
| `ModelBindingContextExtensions.cs` | `ModelBindingContext` 上下文工具方法（错误填充、值提取） |

## For AI Agents

### Working in this directory
- 命名空间使用 `Microsoft.AspNetCore.Http` 等而非 `Poxiao.*`，**保持现状**。
- 添加 `HttpContext` 扩展时优先复用 `GetMetadata<TAttribute>` 而不是手动 `Endpoint.Metadata.GetMetadata`。
- 新增 MVC 过滤器扩展遵循 `AddMvcFilter<TFilter>` 的约定（同时给 `IMvcBuilder` 与 `IServiceCollection` 提供重载）。

### Common patterns
- 静态扩展 + `[SuppressSniffer]`；条件链 `httpContext.GetEndpoint()?.Metadata?...` 全部空安全。

## Dependencies
### Internal
- `Poxiao.FriendlyException`、`Poxiao.SensitiveDetection`、`Poxiao.AspNetCore`（绑定器）
### External
- `Microsoft.AspNetCore.Mvc`、`Microsoft.AspNetCore.Http`、`Microsoft.AspNetCore.Mvc.Controllers`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
