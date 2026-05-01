<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Binders

## Purpose
`[FromConvert]` 的运行时实现：`FromConvertBinder` 持有 `Type → IModelConvertBinder` 的并发字典，逐个参数解析其类型并调用对应转换器；`FromConvertBinderProvider` 实现 `IModelBinderProvider` 把绑定器接入 ASP.NET Core 模型绑定管道。

## Key Files
| File | Description |
|------|-------------|
| `FromConvertBinder.cs` | `IModelBinder` 实现，构造函数注入 `ConcurrentDictionary<Type, Type>`；`BindModelAsync` 中查表→反射 `Activator.CreateInstance`→调用 `ConvertTo` |
| `FromConvertBinderProvider.cs` | `IModelBinderProvider` 实现，根据 `BindingInfo.BinderType` 选择 `FromConvertBinder` |

## For AI Agents

### Working in this directory
- 字典实例由框架在启动阶段注入；业务侧扩展类型映射应通过 `FromConvertBinder` 暴露的注册扩展方法（位于 `App/Extensions`）完成，不在此处硬编码。
- 出现绑定异常时务必给 `bindingContext.ModelState` 写错误项，否则 MVC 不会标记 400。

### Common patterns
- 反射创建转换器实例 + `ValueProviderResult.FirstValue` 取值。

## Dependencies
### Internal
- `AspNetCore/ModelBinders/Attributes`、`AspNetCore/ModelBinders/Converts`
### External
- `Microsoft.AspNetCore.Mvc.ModelBinding`、`Microsoft.Extensions.DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
