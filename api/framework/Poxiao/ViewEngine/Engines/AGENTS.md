<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Engines

## Purpose
ViewEngine 主入口。`IViewEngine` 暴露 12+ 个方法（同步/异步 × 是否泛型模型 × 是否走磁盘缓存），由默认实现 `ViewEngine` 通过 `RazorProjectEngine` 把模板转 C# 代码，再用 `CSharpCompilation.Emit` 输出内存程序集。

## Key Files
| File | Description |
|------|-------------|
| `IViewEngine.cs` | 引擎抽象：`RunCompile` / `RunCompileAsync` / `RunCompileFromCached*` / `Compile<T>` 等。 |
| `ViewEngine.cs` | 默认实现，内部 `CreateAndCompileToStream` 与 `WriteDirectives` 把 `@inherits/@using` 注入 + Roslyn 编译；缓存场景用 `MD5Encryption.Encrypt(content)` 作为文件名。 |

## For AI Agents

### Working in this directory
- DI 注入：`AddViewEngine()` 注册为 `Transient`（见 Extensions），不要改成 Singleton——`MemoryStream` 缓存等内部状态不适合共享。
- 编译失败 → `ViewEngineTemplateException`，含 `GeneratedCode`，调试时优先打印 `GeneratedCode` 而非 `Message`。
- `CreateAndCompileToStream` 使用 `unsafe` + `TryGetRawMetadata` 直接读程序集元数据，效率高但要求程序集为 ReadyToRun/普通托管，不支持单文件发布的捆绑场景未处理资源；如部署 SingleFile 需评估。
- 大模板/高并发：必走 `RunCompileFromCached*`，否则每请求一次 Roslyn 全量编译。

### Common patterns
- 同步重载内部都是 `RunCompileAsync(...).GetAwaiter().GetResult()` 包装；ASP.NET 异步上下文优先调 Async 版本。
- `ViewEngineModel<T>` 用作泛型模型基类，`Compile<ViewEngineModel<T>>` 让模板里 `@Model.Xxx` 强类型可用。

## Dependencies
### Internal
- `ViewEngineOptions`、`ViewEngineTemplate`、`ViewEngineModel`、`Penetrates`、`Poxiao.DataEncryption.MD5Encryption`。
### External
- `Microsoft.AspNetCore.Razor.Language`、`Microsoft.CodeAnalysis.CSharp`、`System.Reflection.Metadata`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
