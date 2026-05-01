<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SingleFile

## Purpose
解决 .NET 单文件发布（`PublishSingleFile=true`）时无法通过文件系统枚举 DLL 的问题：业务侧实现 `ISingleFilePublish` 显式列出需要扫描的程序集，框架的 `App.GetAssemblies()` 在单文件场景下读取此清单。

## Key Files
| File | Description |
|------|-------------|
| `ISingleFilePublish.cs` | `IncludeAssemblies()` 返回 `Assembly[]`、`IncludeAssemblyNames()` 返回字符串名，二选一即可补全单文件场景的程序集集合 |

## For AI Agents

### Working in this directory
- 业务侧若计划单文件发布，必须新增一个实现 `ISingleFilePublish` 的类，列出包含动态 API/`AppStartup`/`IComponent` 的程序集。
- 不要在框架内部添加业务程序集名——把扩展点保留给应用层。

### Common patterns
- 双轨 API：既支持运行时 `Assembly` 引用，也支持仅程序集名称的字符串列表，便于不同部署形态。

## Dependencies
### External
- `System.Reflection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
