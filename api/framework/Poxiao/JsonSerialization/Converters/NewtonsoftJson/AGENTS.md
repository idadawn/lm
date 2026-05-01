<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# NewtonsoftJson

## Purpose
基于 `Newtonsoft.Json` 的 JsonConverter 集合，覆盖检测室系统在 Newtonsoft 调用栈（如 RabbitMQ 消息体、第三方 SDK、`LoggingMonitor` 内部序列化）的四类常见问题：bool 兼容、枚举值/名互转、`DateTime`/`DateOnly`/`TimeOnly` 中文格式、`long` 精度、`Clay` 动态对象。

## Key Files
| File | Description |
|------|-------------|
| `BoolJsonConverter.cs` | 写出转 int(0/1)，读入兼容 `true/false/yes/no/y/n/1/0`。 |
| `EnumJsonConvert.cs` | 枚举与字符串/整数的双向兼容转换器。 |
| `NewtonsoftDateTimeJsonConverter.cs` | `DateTimeConverterBase` 派生，默认按 Unix 毫秒时间戳读写，兼容字符串/整数/日期 token。 |
| `NewtonsoftJsonClayJsonConverter.cs` | `Clay` 动态对象的读写支持。 |
| `NewtonsoftJsonDateOnlyJsonConverter.cs` | `DateOnly`/`DateOnly?` 转换。 |
| `NewtonsoftJsonTimeOnlyJsonConverter.cs` | `TimeOnly`/`TimeOnly?` 转换。 |
| `NewtonsoftJsonLongToStringJsonConverter.cs` | `long`/`long?` 序列化为字符串以避免 JS 53 位精度丢失。 |

## For AI Agents

### Working in this directory
- 与 `SystemTextJson/` 目录功能对位——添加新 Converter 请同步实现两边。
- 注意 `NewtonsoftDateTimeJsonConverter` 用的是 **Unix 毫秒时间戳**，而 SystemTextJson 版本使用 `yyyy-MM-dd HH:mm:ss` 字符串；这是历史差异，修改前确认调用方期望。

### Common patterns
- 全部 `[SuppressSniffer]`；可空版本独立类型；nullable 检测使用 `IsNullableType` 私有助手。

## Dependencies
### External
- `Newtonsoft.Json`、`Newtonsoft.Json.Converters`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
