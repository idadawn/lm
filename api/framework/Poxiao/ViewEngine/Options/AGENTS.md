<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
ViewEngine 编译选项数据类。承载 Roslyn 编译需要的程序集引用、元数据引用、模板命名空间、`@inherits` 默认基类、默认 `using` 集合。每次编译都会 new 一份，并由 `IViewEngineOptionsBuilder` 增量装配。

## Key Files
| File | Description |
|------|-------------|
| `ViewEngineOptions.cs` | 默认引用 `object` 程序集、`ViewEngineModel` 程序集、`Microsoft.CSharp`、`System.Runtime/Linq/Linq.Expressions/Collections`；`TemplateNamespace = "Poxiao.ViewEngine"`、`Inherits = "Poxiao.ViewEngine.Template.Models"`、默认 using 三件套。 |

## For AI Agents

### Working in this directory
- 默认 `Inherits` 指向 `Poxiao.ViewEngine.Template.Models`——这是一个约定符号，由 `ViewEngineOptionsBuilder.Inherits(typeof(ViewEngineModel))` 在编译期重写为真正类型；改默认值会破坏 `Compile(content)` 无泛型 fallback 路径。
- 添加默认引用程序集应改 `ReferencedAssemblies` 初始化集合；运行期建议走 `IViewEngineOptionsBuilder.AddAssemblyReference`，避免影响其它租户。
- `TemplateNamespace` 改名后需同步 `ViewEngineTemplate` 中 `Reflect.GetType(stream, "Poxiao.ViewEngine.Template")` 的查找字符串。

### Common patterns
- 通过 `Reflect.GetAssembly("Microsoft.CSharp")` 等动态加载，避免硬编码 Assembly.Load 失败。

## Dependencies
### Internal
- `Poxiao.Reflection.Reflect`、`ViewEngineModel`。
### External
- `Microsoft.CodeAnalysis.MetadataReference`、`System.Reflection.Assembly`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
