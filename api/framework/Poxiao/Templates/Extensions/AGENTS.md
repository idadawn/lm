<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
`string.Render()` 模板渲染拓展，框架内被多处复用（`SpareTimeAttribute`、`SensitiveDetectionProvider` 等）。支持两种语法：`{prop.subprop}` 占位（来源于对象/字典）与 `#(Config:Section:Key)` 占位（来源于 `App.Configuration`）。

## Key Files
| File | Description |
|------|-------------|
| `StringRenderExtensions.cs` | 三个 `Render` 重载：`(template, object, encode)` / `(template, IDictionary, encode)` / `(template, encode)`。常量正则 `\{(?<p>.+?)\}` 与 `\#\((?<p>.*?)\)`；`MatchTemplateValue` 支持 `{a.b.c}` 多级取值，递归 `ResolveTemplateValue` + `IsRichPrimitive` 判断。 |

## For AI Agents

### Working in this directory
- `encode=true` 时使用 `Uri.EscapeDataString` 包裹结果，专为 URL 模板拼接设计；HTML 等其他场景不要复用该参数。
- `Render(this string template, bool encode = false)` 无第二参数版本默认从 `App.Configuration` 取值——单元测试需 mock `App.Configuration`。
- 多级路径解析靠 `propery = dataType.GetProperty(name)` 反射，**字段不支持**；需要支持字段时改写 `GetValue` 静态本地函数。

### Common patterns
- 私有 `static object MatchTemplateValue` + 嵌套静态本地函数 `GetValue` 实现递归解析。

## Dependencies
### Internal
- `Poxiao.ClayObject.Extensions.ToDictionary`、`Poxiao.Extensions.IsRichPrimitive`、`App.Configuration`。
### External
- `System.Text.RegularExpressions`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
