<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Converters

## Purpose
Schedule 模块内部 `System.Text.Json` 转换器集合。当前仅提供 `DateTime` 的字符串化转换，确保看板 API、`ConvertToJSON`、持久化序列化在跨时区、Unspecified 时间下输出稳定文本。

## Key Files
| File | Description |
|------|-------------|
| `DateTimeJsonConverter.cs` | `JsonConverter<DateTime>`：`Read` 通过 `DateTime.Parse` 反序列化，`Write` 调用 `value.ToString()`，挂载到 `Penetrates.GetDefaultJsonSerializerOptions`。 |

## For AI Agents

### Working in this directory
- 该转换器为 `internal sealed`，仅供 Schedule 内部使用；不要把它注册到全局 `JsonOptions`，会影响整个 API 的时间格式。
- 如需扩展（如 `Nullable<DateTime>`、`DateTimeOffset`），新增独立转换器，并在 `Penetrates.GetDefaultJsonSerializerOptions` 中追加。

### Common patterns
- 序列化使用本地化 `ToString()`，配合 `ScheduleExtensions.ToUnspecifiedString` 在日志/看板中保持 `yyyy-MM-dd HH:mm:ss.fff`。

## Dependencies
### Internal
- 由 `../Internal/Penetrates.cs` 注册到 `JsonSerializerOptions.Converters`。
### External
- `System.Text.Json` / `System.Text.Json.Serialization`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
