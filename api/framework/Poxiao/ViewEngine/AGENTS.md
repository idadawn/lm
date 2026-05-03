<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ViewEngine

## Purpose
基于 Razor + Roslyn 的运行时字符串模板引擎。把 `@inherits ... @using ... @Model.Foo` 这类 Razor 文本直接编译为内存程序集并执行，输出渲染后的字符串。检测室系统用它做：报告/通知/邮件正文生成、代码生成器的模板渲染（CodeGen 模块）以及报表导出文本片段。

## Key Files
| File | Description |
|------|-------------|
| 见各子目录 | 顶层无源文件，所有实现拆在 Builders / Engines / Templates 等子目录中。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Builders/` | `IViewEngineOptionsBuilder` 与默认实现，配置编译期程序集/继承基类/Using (see `Builders/AGENTS.md`) |
| `Engines/` | `IViewEngine` / `ViewEngine` 主入口（同步/异步/缓存编译） (see `Engines/AGENTS.md`) |
| `Exceptions/` | `ViewEngineException`、`ViewEngineTemplateException` (see `Exceptions/AGENTS.md`) |
| `Extensions/` | `services.AddViewEngine()`，以及把 `string` 链式包装成 ViewEnginePart 的扩展 (see `Extensions/AGENTS.md`) |
| `Internal/` | `ViewEnginePart` 链式构建器、`Penetrates` 缓存目录工具、`AnonymousTypeWrapper` (see `Internal/AGENTS.md`) |
| `Models/` | `IViewEngineModel` / `ViewEngineModel<T>` 模板基类（提供 Write/WriteLiteral/特性写入） (see `Models/AGENTS.md`) |
| `Options/` | `ViewEngineOptions`：默认引用程序集、Using、命名空间、Inherits (see `Options/AGENTS.md`) |
| `Templates/` | `IViewEngineTemplate` / `ViewEngineTemplate<T>` 编译产物的运行/缓存 (see `Templates/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 引擎使用 `RazorProjectEngine + CSharpCompilation.Emit(MemoryStream)`，每次编译会产生新程序集；高频场景必须用 `RunCompileFromCached*` 或预先 `Compile + SaveToFile`，否则进程内存与编译耗时会爆。
- 模板默认继承 `Poxiao.ViewEngine.Template.Models`（来自 `ViewEngineOptions.Inherits`），若自定义需通过 `IViewEngineOptionsBuilder.Inherits(typeof(...))` 显式指定，且 `ViewEngineModel` 必须在程序集引用集合内（`AddAssemblyReference`）。
- `RunCompile<T>(template, anonymousModel)`：匿名模型走 `AnonymousTypeWrapper` 反射访问，不能强类型属性引用——若需要请用具名类。
- 缓存目录在 `AppContext.BaseDirectory/templates/~xxx.dll`（由 `Penetrates.GetTemplateFileName` 控制），部署时确保进程对该目录有写权限。

### Common patterns
- 同步 API 内部都是 `async.GetAwaiter().GetResult()` 包装，避免在 ASP.NET 同步上下文里嵌套调用阻塞死锁——如能则优先用 `*Async` 重载。
- 编译失败抛 `ViewEngineTemplateException`，含 `Errors`（Roslyn `Diagnostic` 列表）与 `GeneratedCode`。

## Dependencies
### Internal
- `Poxiao.DataEncryption.MD5Encryption`（生成缓存文件名）、`Poxiao.Reflection.Reflect`、`Poxiao.Extensions.IsAnonymous`。
### External
- `Microsoft.AspNetCore.Razor.Language`、`Microsoft.CodeAnalysis.CSharp`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
