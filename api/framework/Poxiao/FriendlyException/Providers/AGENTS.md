<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Providers

## Purpose
错误码类型提供器抽象。当业务希望以"运行期"方式补充错误码枚举类型（例如插件加载、外部程序集动态引入）时，可实现 `IErrorCodeTypeProvider` 把额外的枚举注入 `Oops` 的扫描结果。

## Key Files
| File | Description |
|------|-------------|
| `IErrorCodeTypeProvider.cs` | 单属性接口：`Type[] Definitions`。`Oops.GetErrorCodeTypes` 通过 `App.GetService<IErrorCodeTypeProvider>(App.RootServices)` 解析，存在则与 `[ErrorCodeType]` 扫描结果 `Concat` + `Distinct`。 |

## For AI Agents

### Working in this directory
- 注册路径：`AddFriendlyException<TProvider>()` 自动 `AddSingleton<IErrorCodeTypeProvider, TProvider>()`；同一进程仅取一个实现（最后注册者覆盖）。
- 返回的类型必须是 **枚举**；非枚举会被 `Oops` 过滤逻辑误判（`u.IsDefined(typeof(ErrorCodeTypeAttribute), true) && u.IsEnum`）。
- 该接口在 `Oops` 静态构造中读取 —— 提供器实例在首次抛异常前必须可解析，否则错误码字典永远缺失这部分定义。

### Common patterns
- 多模块共用错误码：在统一 Provider 中聚合所有模块的错误码枚举，避免到处 `[ErrorCodeType]`。

## Dependencies
### Internal
- 与 `Oops`、`AddFriendlyException<T>` 协作。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
