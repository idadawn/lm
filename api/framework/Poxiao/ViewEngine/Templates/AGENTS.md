<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Templates

## Purpose
模板编译产物的载体。Roslyn 把 Razor 文本编译为 IL 后进入 `MemoryStream`，这层把它包装成可保存（落盘 / 写入流）、可重载（`LoadFromFile/Stream`）、可执行（`Run/RunAsync`）的对象。`ViewEngineTemplate<T>` 提供强类型模型版本（`Action<T> initializer` 而非 `object model`）。

## Key Files
| File | Description |
|------|-------------|
| `IViewEngineTemplate.cs` | 模板抽象：`SaveToStream/File`、`Run(model)`/`RunAsync` 与泛型版 `IViewEngineTemplate<T>: Run(Action<T>)`。 |
| `ViewEngineTemplate.cs` | 实现两类（非泛型/泛型）；构造时 `Reflect.GetType(stream, "Poxiao.ViewEngine.Template")` 反射出生成类型；`RunAsync` 中匿名模型走 `AnonymousTypeWrapper`。 |

## For AI Agents

### Working in this directory
- 缓存复用：`Compile + SaveToFile("biz_xxx")` 一次，后续 `LoadFromFile("biz_xxx")` 直接载入 IL，省去 Razor + Roslyn。
- 文件路径由 `Penetrates.GetTemplateFileName` 拼装（`AppContext.BaseDirectory/templates/~xxx.dll`），运维要确保该目录持久化。
- `Run(object model)` 路径用 `dynamic Model`；强类型业务请用 `Compile<ViewEngineModel<T>>` + `Run(u => u.Model = ...)`。
- 多次 `Run` 同一个 template 实例会 `Activator.CreateInstance(templateType)` 新建实例，线程安全；但底层 `MemoryStream` 仅缓存 IL，不重复编译。

### Common patterns
- 同步 Save/Run 内部都是 `await ...GetAwaiter().GetResult()`，ASP.NET 上下文请优先 Async。

## Dependencies
### Internal
- `Penetrates`、`AnonymousTypeWrapper`、`ViewEngineModel`、`Poxiao.Reflection.Reflect`、`Poxiao.Extensions.IsAnonymous`。
### External
- `System.IO.MemoryStream/FileStream`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
