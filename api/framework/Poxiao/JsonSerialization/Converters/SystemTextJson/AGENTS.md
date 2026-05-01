<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SystemTextJson

## Purpose
基于 `System.Text.Json` 的 `JsonConverter<T>` 集合，是 `IJsonSerializerProvider` 默认实现 (`SystemTextJsonSerializerProvider`) 走的转换链。覆盖检测室系统对外 API 的中文时间格式、`DateOnly`/`TimeOnly` 支持、`long` 精度问题、`Clay` 动态对象。

## Key Files
| File | Description |
|------|-------------|
| `SystemTextJsonClayJsonConverter.cs` | `Clay` 动态对象读写。 |
| `SystemTextJsonDateTimeJsonConverter.cs` | `DateTime`/`DateTime?`，默认格式 `yyyy-MM-dd HH:mm:ss`，构造可注入自定义 Format。 |
| `SystemTextJsonDateTimeOffsetJsonConverter.cs` | `DateTimeOffset`/`DateTimeOffset?` 转换。 |
| `SystemTextJsonDateOnlyJsonConverter.cs` | `DateOnly`/`DateOnly?` 转换。 |
| `SystemTextJsonTimeOnlyJsonConverter.cs` | `TimeOnly`/`TimeOnly?` 转换。 |
| `SystemTextJsonLongToStringJsonConverter.cs` | `long`/`long?` 写为字符串、读时同时接受 string 与 number。 |

## For AI Agents

### Working in this directory
- 默认时间格式为 `yyyy-MM-dd HH:mm:ss`；如要改全局格式，在 Provider 配置 `JsonOptions` 而不是改 Converter 默认值。
- 可空版本是独立类（`*NullableXxx`），不要重构成 `JsonConverter<T?>`——与 Newtonsoft 侧保持对称。
- 全部 `[SuppressSniffer]`，避免被全局类型嗅探注册重复。

### Common patterns
- `Read` 直接用 `Convert.ToDateTime(reader.GetString())`；`Write` 走 `value.ToString(Format)`。`long` 转换器在反序列化时同时支持 `JsonTokenType.String` 与数值。

## Dependencies
### External
- `System.Text.Json`、`System.Text.Json.Serialization`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
