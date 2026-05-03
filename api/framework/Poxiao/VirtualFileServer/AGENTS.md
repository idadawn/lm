<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# VirtualFileServer

## Purpose
统一文件提供器抽象。把 ASP.NET Core 的 `PhysicalFileProvider`（物理盘）与 `EmbeddedFileProvider`（程序集嵌入资源）放在同一个解析委托后面，业务侧 `FS.GetPhysicalFileProvider(path)` / `FS.GetEmbeddedFileProvider(assembly)` 一致调用，且自带为后端常见自定义 MIME（apk、m3u8、et、dps 等）扩展过的 `FileExtensionContentTypeProvider`。

## Key Files
| File | Description |
|------|-------------|
| `FS.cs` | 静态门面：`GetPhysicalFileProvider`/`GetEmbeddedFileProvider`/`GetFileProvider`、`TryGetContentType(fileName, out)`、`GetFileExtensionContentTypeProvider()`（在默认 MIME 表上叠加 .apk/.pem/.m3u8/.et/.dps/.cdr/.gzip/.7zip/.iec/.patch/.bcmap/.shtml/.php* 等映射）。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Enums/` | `FileProviderTypes` 枚举 (see `Enums/AGENTS.md`) |
| `Extensions/` | `AddVirtualFileServer` DI 注册与 `UseVirtualFileServer` 占位中间件 (see `Extensions/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 文件下载/静态资源暴露场景，必须经过 `FS.TryGetContentType` 取 MIME；直接用框架默认 `FileExtensionContentTypeProvider` 会缺 .apk 等映射导致前端无法识别。
- `GetFileProvider` 通过 DI 解析 `Func<FileProviderTypes, object, IFileProvider>` 委托；如需新增提供器类型（如 OSS、MinIO），需要修改 `Extensions/VirtualFileServerServiceCollectionExtensions` 的 switch。
- `UseVirtualFileServer` 当前为占位（直接返回 `app`），不要假设它注册了任何中间件；静态文件请显式 `app.UseStaticFiles(...)`。

### Common patterns
- 上传/导出模块（如 lab 报告附件、APK 渠道包分发）通常通过 `FS` + `IFileProvider` 暴露文件流。

## Dependencies
### Internal
- 由 `api/src/modularity/common/Poxiao.Common.Core/Manager/Files`、`api/src/modularity/upload`、APK 分发等模块使用。
### External
- `Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider`、`Microsoft.Extensions.FileProviders`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
