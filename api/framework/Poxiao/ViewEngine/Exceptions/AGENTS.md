<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Exceptions

## Purpose
ViewEngine 模块异常类型层级。基础 `ViewEngineException` 用于通用错误，`ViewEngineTemplateException` 在 Razor → C# → IL 编译失败时抛出，并携带 Roslyn `Diagnostic` 列表与生成的 C# 源码用于排错。

## Key Files
| File | Description |
|------|-------------|
| `ViewEngineException.cs` | 标记 `[SuppressSniffer]`；提供常规四种构造（默认/消息/消息+内异常/序列化）。 |
| `ViewEngineTemplateException.cs` | 派生自 `ViewEngineException`，额外 `Errors: List<Diagnostic>`、`GeneratedCode: string`；`Message` 重写为聚合所有 Error/WarningAsError 诊断。 |

## For AI Agents

### Working in this directory
- 调试模板编译失败：`catch (ViewEngineTemplateException ex)`，先看 `ex.GeneratedCode`（被注入了 `@inherits`/`@using` 的真实 C# 源），再看 `ex.Errors`。
- 业务代码不应吞掉这两个异常 → 业务方把模板渲染包在 `try/catch` 后用 `AppFriendlyException` 重抛，给前端 RESTfulResult 输出友好信息。
- `[Serializable]` 受跨进程序列化场景使用；通常应用不需要单独构造。

### Common patterns
- 捕获后通常输出 `GeneratedCode` 到 Serilog Debug 通道，避免日志泛滥。

## Dependencies
### Internal
- 由 `../Engines/ViewEngine.CreateAndCompileToStream` 抛出。
### External
- `Microsoft.CodeAnalysis.Diagnostic`、`System.Runtime.Serialization`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
