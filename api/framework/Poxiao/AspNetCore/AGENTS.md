<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# AspNetCore

## Purpose
对 ASP.NET Core 原生抽象的薄封装，提供 `HttpContext`/`IHost`/`ModelBindingContext` 工具方法，以及自定义 `[FromConvert]` 模型绑定体系——允许通过 `IModelConvertBinder` 把字符串原值转成强类型（如时间反 URL 转义）。

## Key Files
（无直接文件，全部位于子目录）

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | `HttpContext`、`IHost`、`AspNetCoreBuilder` 等扩展 (see `Extensions/AGENTS.md`) |
| `ModelBinders/` | 自定义模型绑定特性、绑定器与转换器实现 (see `ModelBinders/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 任何 ASP.NET Core 公共扩展都放在 `Extensions/`，模型绑定相关放在 `ModelBinders/`，保持目录语义清晰。
- 扩展方法的命名空间故意采用 `Microsoft.*`（例如 `Microsoft.AspNetCore.Http`、`Microsoft.AspNetCore.Mvc`），以便业务代码免显式 `using Poxiao.AspNetCore`。

### Common patterns
- 扩展类标 `[SuppressSniffer]`，避免被反射 DI 误注册。
- 绑定器与转换器通过 `ConcurrentDictionary<Type, Type>` 分发，运行期可线程安全扩展。

## Dependencies
### Internal
- `Poxiao.FriendlyException`、`Poxiao.SensitiveDetection`
### External
- `Microsoft.AspNetCore.*`、`Microsoft.CodeAnalysis`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
