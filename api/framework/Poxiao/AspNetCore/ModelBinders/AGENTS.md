<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ModelBinders

## Purpose
框架的自定义模型绑定基础设施，让业务方法签名上写 `[FromConvert]` 即可调用注册的 `IModelConvertBinder` 把原始字符串转换成强类型（典型场景：URL 编码的中文时间、特殊格式日期、`DateTime`/`DateTimeOffset` 等）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[FromConvert]` 绑定特性定义 (see `Attributes/AGENTS.md`) |
| `Binders/` | `IModelBinder` + `IModelBinderProvider` 实现 (see `Binders/AGENTS.md`) |
| `Converts/` | `IModelConvertBinder` 接口与内置 `DateTime`/`DateTimeOffset` 转换器 (see `Converts/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 三层结构不可乱放：特性→`Attributes/`、绑定器→`Binders/`、单类型转换器→`Converts/`。
- 注册自定义转换器：在业务的 `AppStartup.ConfigureServices` 里把 `Type → IModelConvertBinder` 加入到 `FromConvertBinder` 使用的 `ConcurrentDictionary<Type, Type>` 中。
- 不要破坏 `[FromConvert].Customize=true` 的“完全自定义”语义——框架会跳过内部处理。

### Common patterns
- 绑定器构造函数注入字典实例；`IModelBinderProvider` 在 MVC 配置阶段把绑定器登记到 `MvcOptions.ModelBinderProviders`。

## Dependencies
### External
- `Microsoft.AspNetCore.Mvc.ModelBinding`、`Microsoft.AspNetCore.Mvc.ModelBinding.Metadata`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
