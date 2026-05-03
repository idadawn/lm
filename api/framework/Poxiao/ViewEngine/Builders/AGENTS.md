<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Builders

## Purpose
ViewEngine 编译选项的链式构建器。在每次 `Compile`/`RunCompile` 之前，业务通过 `Action<IViewEngineOptionsBuilder>` 注册额外程序集引用、命名空间 using、模板继承基类与 Roslyn `MetadataReference`。

## Key Files
| File | Description |
|------|-------------|
| `IViewEngineOptionsBuilder.cs` | 接口：`AddAssemblyReferenceByName/Assembly/Type`、`AddMetadataReference`、`AddUsing`、`Inherits(Type)`、`Options` 直读。 |
| `ViewEngineOptionsBuilder.cs` | 默认实现；`AddAssemblyReference(Type)` 会递归把泛型参数程序集也加入；`Inherits` 内部调用 `RenderTypeName` 处理泛型/嵌套类型生成 `@inherits` 字符串。 |

## For AI Agents

### Working in this directory
- 自定义模板基类时必须 `builder.Inherits(typeof(YourBaseModel))`；它会同时把基类所在程序集加入 ReferencedAssemblies，否则 Roslyn 会找不到符号。
- 添加业务 DTO 程序集：`builder.AddAssemblyReference(typeof(YourDto))`，避免在模板里用到却未引用。
- `RenderTypeName` 处理嵌套类型时用 `.` 分隔，不支持 `+`，因此模板继承类不能是嵌套到深层的私有类。

### Common patterns
- ReferencedAssemblies 是 `HashSet<Assembly>`，重复 Add 无副作用。
- 链式构建器实例每次 Compile 都会 `new`，不会线程共享。

## Dependencies
### Internal
- `ViewEngineOptions`、`Poxiao.Reflection.Reflect`。
### External
- `Microsoft.CodeAnalysis.MetadataReference`、`System.Reflection.Assembly`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
