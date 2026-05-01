<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Providers

## Purpose
JSON 序列化抽象的契约 + 默认实现。`IJsonSerializerProvider` 是 `JSON` 静态门面背后的抽象，业务调用 `JSON.Serialize` 时通过 DI 拿到当前注册的 Provider；提供两套实现以便在 STJ/Newtonsoft 之间切换或并存。

## Key Files
| File | Description |
|------|-------------|
| `IJsonSerializerProvider.cs` | 接口契约：`Serialize`、`Deserialize<T>`、`Deserialize(string,Type,...)`、`GetSerializerOptions()`。`jsonSerializerOptions` 用 `object` 弱类型以容纳两套库的不同选项类型。 |
| `SystemTextJsonSerializerProvider.cs` | 默认实现，`[Injection(Order = -999)] ISingleton`；构造注入 `IOptions<JsonOptions>`，全部委托给 `JsonSerializer.Serialize/Deserialize`，`GetSerializerOptions()` 返回 MVC 全局 `JsonSerializerOptions`。 |
| `NewtonsoftJsonSerializerProvider.cs` | 兼容实现，调用 `JsonConvert.SerializeObject/DeserializeObject`；用于切换为 Newtonsoft 默认链或在特定子系统局部注入。 |

## For AI Agents

### Working in this directory
- `[Injection(Order = -999)]` 让默认 STJ Provider 优先注册；编写替代 Provider 时给更小的 Order 才会覆盖。
- `Deserialize<T>` 与 `Deserialize(json, Type, opts)` 必须实现一致，否则反射场景（Web API 模型绑定外的代码）行为分裂。
- `GetSerializerOptions()` 返回 `object`，调用方需要自行强转，注意分别返回 `JsonSerializerOptions` / `JsonSerializerSettings`。

### Common patterns
- 单例 + 弱类型选项参数 + 构造函数注入 `IOptions<>`。

## Dependencies
### Internal
- `Poxiao.App.GetService`、`Injection`/`ISingleton` 标记。

### External
- `System.Text.Json`、`Newtonsoft.Json`、`Microsoft.AspNetCore.Mvc.JsonOptions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
