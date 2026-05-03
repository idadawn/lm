<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
友好异常模块的内部数据结构与日志方法。承载异常运行期元数据 (`ExceptionMetadata`)、缓存的方法异常信息 (`MethodIfException`)、`LoggerMessage` 高性能日志 (`Logging.cs`)。

## Key Files
| File | Description |
|------|-------------|
| `ExceptionMetadata.cs` | `sealed`，所有 setter `internal`。字段：`StatusCode` / `ErrorCode` / `OriginErrorCode` / `Errors`（错误对象信息）/ `Data`。由 `UnifyContext.GetExceptionMetadata(ExceptionContext)` 统一构造，传给 `FriendlyExceptionFilter` 与 `BadPageResult`。 |
| `MethodIfException.cs` | `internal sealed`。字段 `MethodBase ErrorMethod`、`IEnumerable<IfExceptionAttribute> IfExceptionAttributes`。被 `Oops.GetEndPointExceptionMethod` 用 `ConcurrentDictionary` 缓存，避免每次抛异常都遍历堆栈反射。 |
| `Logging.cs` | `LoggerMessage` 源生成日志的 `partial` 类伴生文件，与同名公开类合并以提高日志吞吐。 |

## For AI Agents

### Working in this directory
- 这些类型是 `internal` —— 不在公共 API 表面，重命名/移除时只需保证 `Oops` / `FriendlyExceptionFilter` / `UnifyContext` 等内部调用方同步更新。
- `MethodIfException` 缓存键是 `MethodBase` 实例 —— `Oops.ErrorMethods` 是进程级别 `ConcurrentDictionary<MethodBase, MethodIfException>`，不会过期。

### Common patterns
- `ExceptionMetadata.Errors` 是 `object` —— 可承载字符串、字典、`ProblemDetails` 等，规范化结果序列化时再决定形态。

## Dependencies
### Internal
- `IfExceptionAttribute`、`UnifyContext`（`Poxiao.UnifyResult`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
