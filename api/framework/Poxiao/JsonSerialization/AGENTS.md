<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# JsonSerialization

## Purpose
`Poxiao` 框架统一的 JSON 序列化抽象层。通过 `IJsonSerializerProvider` 把 `System.Text.Json` 与 `Newtonsoft.Json` 统一在同一接口背后，让上层（控制器、日志、缓存、消息总线）只依赖 `JSON.Serialize/Deserialize` 静态门面，避免直接耦合具体序列化器。检测室系统的 long ID 精度、`DateTime`/`DateOnly`/`TimeOnly` 中文显示、`Clay` 动态对象都靠此处的 Converter 解决。

## Key Files
| File | Description |
|------|-------------|
| `JSON.cs` | 静态门面，封装 `Serialize`、`Deserialize<T>`、`GetSerializerOptions<TOptions>()`，内部 `App.GetService<IJsonSerializerProvider>()`。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Providers/` | `IJsonSerializerProvider` 接口 + System.Text.Json/Newtonsoft 两套实现 (see `Providers/AGENTS.md`) |
| `Converters/` | 自定义 JsonConverter，按底层库分两个子目录 (see `Converters/AGENTS.md`) |
| `Extensions/` | DI/Builder 扩展，注册 Provider、配置 `JsonOptions` (see `Extensions/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 不要在调用点直接 `new JsonSerializerOptions()`；统一通过 `JSON.GetSerializerOptions<JsonSerializerOptions>()` 取全局配置。
- 新增 Provider 必须实现 `IJsonSerializerProvider` 并标 `[Injection(Order = ...)]`、`ISingleton`，注册顺序决定默认实现（`SystemTextJsonSerializerProvider` 默认 -999）。

### Common patterns
- 静态门面 + DI Provider；`object jsonSerializerOptions = default` 形参为弱类型以兼容两套库。

## Dependencies
### Internal
- `Poxiao.App` 服务定位、`Injection`/`ISingleton` DI 标记。

### External
- `System.Text.Json`、`Newtonsoft.Json`、`Microsoft.AspNetCore.Mvc.JsonOptions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
