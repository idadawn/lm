<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Converters

## Purpose
JSON 自定义转换器集合的根目录，按底层序列化库分仓：`SystemTextJson/`（默认）与 `NewtonsoftJson/`（兼容老 API/三方库）。两套都需要解决检测室系统遇到的相同 4 类问题：`long` 精度（JS Number 53 位）、`DateTime` 中文格式、`DateOnly`/`TimeOnly` 缺失原生支持、`Clay` 动态对象。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `SystemTextJson/` | `JsonConverter<T>` 实现（默认） (see `SystemTextJson/AGENTS.md`) |
| `NewtonsoftJson/` | `JsonConverter` 实现（兼容路径） (see `NewtonsoftJson/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增 Converter 时，**两套库都要镜像实现**，否则同一类型在不同栈表现不一致；命名前缀分别为 `SystemTextJson*`/`Newtonsoft*`。
- 所有 Converter 必须 `[SuppressSniffer]`，避免被框架类型扫描误注册。
- 可空类型单独写 `*NullableXxxJsonConverter` 而不是用 `JsonConverter<T?>`，与现有命名一致。

## Dependencies
### Internal
- 上级 `JsonSerialization` 通过 Provider 注册到 `JsonSerializerOptions.Converters`。

### External
- `System.Text.Json.Serialization`、`Newtonsoft.Json`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
