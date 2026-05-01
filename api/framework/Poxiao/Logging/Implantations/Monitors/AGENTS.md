<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Monitors

## Purpose
"日志监视器"切面：MVC `IAsyncActionFilter` + `IAsyncPageFilter`，拦截每一次 Action / Razor Page 请求，把控制器名、动作名、URL、Header、客户端 IP、参数、返回值、异常、JWT Claims、执行耗时、系统/启动信息等约 30 个字段以**结构化 JSON + 可读文本**两种形式写入日志。检测室系统的接口请求审计、问题排查全靠它。

## Key Files
| File | Description |
|------|-------------|
| `LoggingMonitorAttribute.cs` | 核心切面，可贴在方法/类（`AllowMultiple=false`、`Inherited=true`），全局也可通过 `LoggingMonitorSettings` 注入；约 1000 行覆盖：JWT 模板、参数模板（含 `IFormFile`/`byte[]`）、返回值模板（含阈值截断 `ReturnValueThreshold`）、异常模板（区分 `AppFriendlyException` 验证异常）。 |
| `LoggingMonitorSettings.cs` | 全局配置：`GlobalEnabled`、`IncludeOfMethods`/`ExcludeOfMethods`、`MethodsSettings`、`InternalWriteFilter`、`Configure` 委托、`BahLogLevel`（业务异常落库级别）。 |
| `LoggingMonitorMethod.cs` | 方法级覆盖配置（`FullName` + 单方法的 `WithReturnValue`/`JsonBehavior`/`IgnorePropertyNames` 等）。 |
| `JsonBehavior.cs` | 枚举 `OnlyJson`/`OnlyText`/`Both`，控制最终写入的消息格式。 |
| `ContractResolverTypes.cs` | 枚举 `Default`/`CamelCase`，决定 Newtonsoft 序列化命名规则。 |
| `PropertyNamesContractResolver.cs` | `DefaultContractResolverWithIgnoreProperties` + `CamelCasePropertyNamesContractResolverWithIgnoreProperties`：按名字/类型忽略属性。 |
| `Logging.cs` | `internal sealed class LoggingMonitor`：仅作日志分类名占位（`ILogger<LoggingMonitor>`）。 |
| `SuppressMonitorAttribute.cs` | 类/方法/参数级"跳过监视器"开关，参数级用于阻止 `[FromServices]` 之外的敏感参数被序列化。 |

## For AI Agents

### Working in this directory
- 全局开关与方法级开关同时配置时，方法级 `[LoggingMonitor]` 优先；若 `Settings.FromGlobalFilter == true` 而方法已贴特性，全局触发会被跳过避免双写。
- 参数序列化使用 Newtonsoft（`JsonConvert.SerializeObject` + `*WithIgnoreProperties` resolver）；`long` 通过 `Converters.AddLongTypeConverters()` 转字符串避免精度丢失。
- 返回值阈值 `ReturnValueThreshold > 0` 时被截断，写入 JSON 用 `WriteStringValue` 而不是 `WriteRawValue` 以防 JSON 损坏。
- `[SuppressMonitor]` 的覆盖范围是类/方法/参数；新增"敏感字段"屏蔽请优先用此特性而不是修改 `IgnorePropertyNames`。

### Common patterns
- 一切信息先写入 `Utf8JsonWriter` 流 + 同时 push 文本模板列表，最后用 `TP.Wrapper` 拼成可读文本；按 `JsonBehavior` 选择最终输出。

## Dependencies
### Internal
- `Poxiao.UnifyResult.UnifyContext`、`Poxiao.FriendlyException.AppFriendlyException`、`Poxiao.DataValidation.DataValidationFilter`、`Poxiao.Templates.TP`、`Poxiao.Extensions.HttpContext*`。

### External
- `Microsoft.AspNetCore.Mvc.Filters`、`Newtonsoft.Json`、`System.Text.Json`、`System.Diagnostics.Stopwatch`、`System.Runtime.InteropServices`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
