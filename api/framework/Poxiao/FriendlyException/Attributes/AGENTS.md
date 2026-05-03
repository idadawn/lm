<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
友好异常的元数据特性。包括错误码定义（枚举级 + 字段级）与方法/类级别的"异常复写"特性，让错误消息与堆栈位置解耦。

## Key Files
| File | Description |
|------|-------------|
| `ErrorCodeTypeAttribute.cs` | `[ErrorCodeType]` 标记**枚举类型**为错误码定义源；`Oops.GetErrorCodeTypes` 通过 `App.EffectiveTypes` + `IsEnum` 收集，并合并 `IErrorCodeTypeProvider.Definitions`。 |
| `ErrorCodeItemMetadataAttribute.cs` | `[ErrorCodeItemMetadata("消息 {0}", arg1)]` 贴在枚举字段上。属性：`ErrorMessage`、`ErrorCode`（可显式指定，否则用字段名）、`Args`（格式化参数）。 |
| `IfExceptionAttribute.cs` | 方法/类级（`AllowMultiple = true`）的"异常复写"。三种构造：默认、`(errorCode, args)` 复写消息、`(exceptionType)` 仅按异常类型挂全局处理；属性 `ErrorCode` / `ExceptionType` / `ErrorMessage` / `Args`。 |

## For AI Agents

### Working in this directory
- 错误码枚举一般放在业务模块内并贴 `[ErrorCodeType]` —— 框架启动时一次性扫描，业务运行期新增不会被收录。
- `ErrorCodeItemMetadata.ErrorCode` 可显式覆盖字段名，常用于把数字码（如 `10001`）映射到可读字段。
- `[IfException]` 没有匹配 `ErrorCode` 时充当兜底：`Oops` 会回退取首个 `ErrorCode == null && !string.IsNullOrWhiteSpace(ErrorMessage)` 的特性。

### Common patterns
- 所有特性均带 `[SuppressSniffer]`，避免被自动扫描误识别为可注入服务。

## Dependencies
### Internal
- 被 `Oops`、`MethodIfException`、`ErrorCodeMessageSettingsOptions` 配合使用。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
