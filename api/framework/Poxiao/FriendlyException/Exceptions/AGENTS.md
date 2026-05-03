<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Exceptions

## Purpose
框架自定义异常类型。`AppFriendlyException` 是所有 `Oops.Oh` / `Oops.Bah` 抛出的实际异常，承载错误码、状态码、是否为校验异常等元数据，被 MVC 过滤器与统一返回结果识别。

## Key Files
| File | Description |
|------|-------------|
| `AppFriendlyException.cs` | 继承 `System.Exception`。属性：`ErrorCode` / `OriginErrorCode`（前者可被 `[IfException]` 复写）、`ErrorMessage`（`object` 支持复杂对象）、`Args`、`StatusCode`（默认 `500`，由 `StatusCode()` 拓展或 `Bah` 改写为 `400`）、`ValidationException`、`Data`（额外数据，`new` 隐藏父类同名成员）。 |

## For AI Agents

### Working in this directory
- 不要在用户代码 `new AppFriendlyException`；用 `Oops.Oh / Oops.Bah` 入口，可享受错误码字典 + `[IfException]` 复写 + 本地化等行为。
- 该异常被 `FriendlyExceptionFilter`、`UnifyContext.GetExceptionMetadata`、`PageResult` 渲染等多处 `is` 检查 —— 派生子类时务必保留属性语义。
- `ValidationException = true` 会跳过 `IGlobalExceptionHandler` 调用与日志告警，仅作 400 校验返回；服务异常应保持 `false`。

### Common patterns
- `Data` 字段常承载结构化错误对象（字段错误列表等），与 `WithData(...)` 拓展配合。

## Dependencies
### Internal
- 与 `Oops`、`AppFriendlyExceptionExtensions`、`FriendlyExceptionFilter`、`UnifyResult` 紧密耦合。
### External
- `Microsoft.AspNetCore.Http.StatusCodes`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
