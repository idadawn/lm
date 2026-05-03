<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
ViewEngine 内部支撑代码：链式构建器 `ViewEnginePart` 的属性/方法/配置三个 partial 文件、匿名类型适配器与缓存目录工具。这些类型不对外公开实现细节，但承担了字符串扩展（`SetTemplateModel`/`RunCompile`）的真实状态机。

## Key Files
| File | Description |
|------|-------------|
| `ViewEnginePart.cs` | partial 主体：`Template`、`TemplateOptionsBuilder`、`TemplateModel: (Type, object)`、`TemplateCachedFileName`、`ViewEngineScoped`。 |
| `ViewEnginePartSetters.cs` | 链式 setter（SetTemplate / SetTemplateModel / SetTemplateOptionsBuilder / SetTemplateCachedFileName / SetViewEngineScoped）。 |
| `ViewEnginePartMethods.cs` | 终结操作：`RunCompile[Async]`、`RunCompileFromCached[Async]` 解析 IViewEngine 并执行。 |
| `AnonymousTypeWrapper.cs` | 把匿名对象（C# 编译器隐藏成员可见性）包装成 `IDictionary` 般可访问的代理，供 Razor 模板内部反射读属性。 |
| `Penetrates.cs` | `GetTemplateFileName(name)` 计算 `AppContext.BaseDirectory/templates/~xxx.dll` 缓存路径，确保目录存在。 |

## For AI Agents

### Working in this directory
- 改动 `ViewEnginePart` 任一文件务必同步另两个 partial（命名 setter/methods 文件即明示职责切分）。
- `Penetrates.GetTemplateFileName` 强制把扩展名补成 `.dll` 并加 `~` 前缀，移植到容器/无写权限环境时记得映射 `templates/` 卷。
- `AnonymousTypeWrapper` 仅在 `RunCompile(template, anonModel)` 路径触发（`model.IsAnonymous() == true`），强类型模型不会经过它。

### Common patterns
- partial 文件按"状态/setter/methods"切分是模块约定。
- 缓存名默认 `MD5Encryption.Encrypt(content)`；字符级改动会让缓存命中失效。

## Dependencies
### Internal
- `IViewEngine`、`ViewEngineModel`、`Poxiao.Extensions.IsAnonymous`、`Poxiao.DataEncryption.MD5Encryption`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
