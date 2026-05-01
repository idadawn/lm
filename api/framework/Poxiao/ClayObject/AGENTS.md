<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ClayObject

## Purpose
框架的“粘土对象”动态数据类型，类似 JavaScript 对象/数组：`Clay` 继承 `DynamicObject`，内部用 `XElement` 存储，对外通过 `dynamic` 关键字访问任意属性、迭代成员。常用于动态拼装 JSON 响应、解析未知结构数据、动态 API 入参。

## Key Files
| File | Description |
|------|-------------|
| `Clay.cs` | 动态对象核心：构造函数支持 `throwOnUndefined`，对象/数组双形态，集成 `JsonSerialization`、`XElement` 转换、`Newtonsoft.Json` 互操作 |
| `ClayObjectEnumerator.cs` | 对象形态成员枚举器（`KeyValuePair<string, dynamic>`） |
| `ClayArrayEnumerator.cs` | 数组形态成员枚举器（`dynamic` 元素） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Extensions/` | 字典/`ExpandoObject` 与 `Clay` 互转扩展 (see `Extensions/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 优先使用 `Clay.Parse`/`Clay.Object`/`Clay.Array` 工厂创建实例，不要直接 `new` 私有构造。
- `throwOnUndefined=false` 时缺失属性返回 `null` 而非抛异常——业务侧需要的话显式传 `false`。
- 不要把 `Clay` 暴露在跨进程/RPC 边界（如 RabbitMQ 消息），优先序列化为常规 JSON。

### Common patterns
- `DynamicObject` + `XElement` 实现可结构化的“动态 JSON”；通过 `Newtonsoft.Json.Linq.JObject`/`System.Text.Json.JsonElement` 双向桥接。
- `[SuppressSniffer]` 标注，避免被 DI 扫描误注册。

## Dependencies
### Internal
- `Poxiao.JsonSerialization`、`Poxiao.Extensions`
### External
- `Newtonsoft.Json`、`System.Xml.Linq`、`System.Runtime.Serialization.Json`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
