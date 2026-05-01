<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
VirtualFileServer 的 DI 与中间件注册扩展。`AddVirtualFileServer` 把"按类型创建 IFileProvider"的工厂委托注入容器，使 `FS.GetFileProvider` 能解析。`UseVirtualFileServer` 是预留中间件位（当前空实现），便于未来加全局虚拟路径映射或鉴权钩子。

## Key Files
| File | Description |
|------|-------------|
| `VirtualFileServerServiceCollectionExtensions.cs` | 注册 Singleton `Func<FileProviderTypes, object, IFileProvider>`：`Physical → new PhysicalFileProvider(args as string)`，`Embedded → new EmbeddedFileProvider(args as Assembly)`，其余抛 `NotSupportedException`。 |
| `VirtualFileServerApplicationBuilderExtensions.cs` | `UseVirtualFileServer(this IApplicationBuilder)` —— 当前直接返回 `app`，作为未来扩展点保留。 |

## For AI Agents

### Working in this directory
- 启动配置须显式 `services.AddVirtualFileServer()`；缺失会让 `FS.GetFileProvider` 在 `App.GetService<Func<...>>` 处返回 null。
- 添加新提供器类型（OSS/MinIO/FTP）：在工厂 switch 内加 case 并完善 `args` 类型转换；同时更新 `Enums/FileProviderTypes` 与 `FS.cs` 便捷方法。
- `UseVirtualFileServer` 不要在内部直接 `UseStaticFiles`；保留为业务装配位，让用户决定挂载顺序与白名单。

### Common patterns
- 工厂返回 `(Func<FileProviderTypes, object, IFileProvider>)fileProviderResolve` 的强转写法是 OMC 项目中其它 Resolve 委托的统一风格（见 `RemoteRequestPart` 等）。

## Dependencies
### Internal
- `FileProviderTypes`、`FS`。
### External
- `Microsoft.Extensions.FileProviders.PhysicalFileProvider/EmbeddedFileProvider`、`Microsoft.Extensions.DependencyInjection`、`Microsoft.AspNetCore.Builder`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
