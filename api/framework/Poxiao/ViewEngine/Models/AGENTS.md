<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Models

## Purpose
模板运行时基类。Razor 编译产物（`Poxiao.ViewEngine.Template`）默认派生自 `ViewEngineModel`，引擎执行时调用 `ExecuteAsync()` 渲染，期间通过 `WriteLiteral` / `Write` / `BeginWriteAttribute` 等回调把片段写入内部 `StringBuilder`，最后由 `ResultAsync()` 输出字符串。

## Key Files
| File | Description |
|------|-------------|
| `IViewEngineModel.cs` | 接口定义所有 Razor 编译器期待的写入回调（同步/异步成对）。 |
| `ViewEngineModel.cs` | 抽象基类实现：内部 `StringBuilder`，处理特性写入（prefix/suffix）、`Model: dynamic`；以及泛型版 `ViewEngineModel<T> : ViewEngineModel`，把 `Model` 收窄成 `T`。 |

## For AI Agents

### Working in this directory
- 自定义模板基类（如增加 Helpers 给模板用）：继承 `ViewEngineModel<T>` 并通过 `IViewEngineOptionsBuilder.Inherits(typeof(YourBase))` 切换。
- 注意 `dynamic Model` 与 `T Model` 通过 `new` 关键字隐藏；模板代码中 `@Model.X` 走的是泛型版本，不要再覆盖回 dynamic。
- 同步方法均使用 `GetAwaiter().GetResult()` 调用对应 Async 重载——基类设计为 async-first，重写时优先实现 Async 版本。

### Common patterns
- 特性写入 (`BeginWriteAttribute` / `WriteAttributeValue` / `EndWriteAttribute`) 通过 `attributeSuffix` 字段保存末尾字符；这是 Razor 编译器的固定调用规范，不要修改语义。

## Dependencies
### Internal
- 由 `../Engines/ViewEngine` 通过 Roslyn 编译器作为 `@inherits` 默认基类引用。
### External
- 仅 `System.Text.StringBuilder`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
