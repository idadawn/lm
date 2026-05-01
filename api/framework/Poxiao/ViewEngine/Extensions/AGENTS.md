<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
ViewEngine 注册与人体工学扩展。`AddViewEngine` 把 `IViewEngine` 加入 DI；`ViewEngineStringExtensions` 在 `string` 上挂一组链式扩展（`SetTemplate*` / `RunCompile*`），让业务代码可以写 `templateString.SetTemplateModel(model).RunCompile()`。

## Key Files
| File | Description |
|------|-------------|
| `ViewEngineServiceCollectionExtensions.cs` | `services.AddTransient<IViewEngine, ViewEngine>()`。 |
| `ViewEngineStringExtensions.cs` | 把 `string` 包装为 `ViewEnginePart`，提供 `SetTemplateModel`、`SetTemplateOptionsBuilder`、`SetTemplateCachedFileName`、`SetViewEngineScoped`、`RunCompile[Async]`、`RunCompileFromCached[Async]` 等。 |

## For AI Agents

### Working in this directory
- 业务代码优先使用字符串扩展（更简洁），需要传 `IServiceProvider scope` 的场景调 `SetViewEngineScoped(provider)`，让模板内部解析依赖时不用 `App.RootServices`。
- 缓存：`SetTemplateCachedFileName("biz_xxx")` 比默认 `MD5(content)` 文件名更易读，但需自行保证模板版本变更时换名（否则取到旧编译产物）。
- 不要把 `IViewEngine` 注册成 Singleton 重复包装；`AddViewEngine` 已经是 Transient。

### Common patterns
- 链式调用每步 `return ViewEnginePart.Default().SetTemplate(template).Set...()`，符合 Poxiao 框架的 `Part` 风格（`MailKitPart`、`CryptogramPart` 等）。

## Dependencies
### Internal
- `IViewEngine`、`ViewEnginePart`。
### External
- `Microsoft.Extensions.DependencyInjection`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
