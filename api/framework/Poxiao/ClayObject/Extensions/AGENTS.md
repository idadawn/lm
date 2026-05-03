<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
`Clay` 与字典/`ExpandoObject` 之间的互转扩展。把 POCO、`JObject`、`JsonElement`、匿名类型等转为 `IDictionary<string, object>`，反向把 `ExpandoObject` 包装为 `Clay`，方便动态拼装 JSON 响应或动态读取未知 schema 数据。

## Key Files
| File | Description |
|------|-------------|
| `DictionaryExtensions.cs` | `object.ToDictionary()` 处理 `JObject`/`POCO`/匿名对象/`IDictionary`；通过反射读取属性 |
| `ExpandoObjectExtensions.cs` | `ExpandoObject` 与 `Clay`/字典互转 |

## For AI Agents

### Working in this directory
- 扩展类标 `[SuppressSniffer]`；命名空间 `Poxiao.ClayObject.Extensions`。
- 反射映射避免循环引用：复杂 POCO 应提前序列化为 JSON 字符串再 `Clay.Parse`。

### Common patterns
- 同一个扩展方法处理多种输入：先按类型分支（`JObject`/`IDictionary`/匿名类型），再做反射兜底。

## Dependencies
### Internal
- `Poxiao.Extensions`、`Poxiao.JsonSerialization`
### External
- `Newtonsoft.Json.Linq`、`System.Text.Json`、`System.Dynamic`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
