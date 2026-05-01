<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Converts

## Purpose
`IModelConvertBinder` 抽象与内置实现，把字符串原始值转换为强类型。框架自带 `DateTime` / `DateTimeOffset` 两种常用时间转换（先 `Uri.UnescapeDataString` 解码再 `Convert.ToDateTime`），业务可实现接口扩展更多类型。

## Key Files
| File | Description |
|------|-------------|
| `IModelConvertBinder.cs` | `object ConvertTo(ModelBindingContext, DefaultModelMetadata, ValueProviderResult, object extras=default)` 单方法接口 |
| `DateTimeModelConvertBinder.cs` | `DateTime` 转换器：处理 URL 编码后的中文/带空格时间字符串 |
| `DateTimeOffsetModelConvertBinder.cs` | `DateTimeOffset` 版本，对应带时区的时间参数 |

## For AI Agents

### Working in this directory
- 新增类型转换器时实现 `IModelConvertBinder` 并放在本目录；命名遵循 `<Type>ModelConvertBinder.cs`。
- 不要在转换器里抛业务异常——返回 `null` 或写 `bindingContext.ModelState` 让 MVC 决定 400 响应。
- 时间相关转换务必保留 `Uri.UnescapeDataString` 一步：业务侧 LIMS 报表查询常带 URL 编码的中文时间。

### Common patterns
- 无状态、可重入；通常 `[SuppressSniffer]` 标注。

## Dependencies
### External
- `Microsoft.AspNetCore.Mvc.ModelBinding`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
