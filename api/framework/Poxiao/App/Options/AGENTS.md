<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Options

## Purpose
应用级运行时选项与启动参数对象。`AppSettingsOptions` 对应 `appsettings.json` 的 `AppSettings` 节点（MiniProfiler、规范化文档、外部程序集、虚拟目录等开关）；`RunOptions` 系列封装 `WebApplication.CreateBuilder` 的链式配置与静默启动模式。

## Key Files
| File | Description |
|------|-------------|
| `AppSettingsOptions.cs` | 全局 `AppSettings` 配置（`InjectMiniProfiler`、`InjectSpecificationDocument`、`ExternalAssemblies`、`ExcludeAssemblies`、`SupportPackageNamePrefixs`、`VirtualPath` 等），`PostConfigure` 给定生产环境兜底 |
| `RunOptions.cs` | `Serve.Run` 的标准选项：`Default`/`Main(args)`/`DefaultSilence`/`MainSilence(args)`，包装 `WebApplicationOptions`/`WebApplicationBuilder`/`WebApplication` 配置回调 |
| `IRunOptions.cs` | 运行选项标记接口 |
| `GenericRunOptions.cs` | 泛型主机（非 Web）启动选项 |
| `LegacyRunOptions.cs` | 兼容旧版 Startup 风格的启动选项 |

## For AI Agents

### Working in this directory
- `AppSettingsOptions.PostConfigure` 中“生产环境且未配置 → 关闭 MiniProfiler”的语义须保留，Web/Worker 边界依赖它。
- 新增 `RunOptions` 钩子时遵循“默认值不可变 + 链式 `With*` 修改”模式，参考 `RunOptions.Default`。
- 不要在 `AppSettingsOptions` 加业务字段，业务配置通过 `IConfigurableOptions<T>` 自定义节点。

### Common patterns
- `IConfigurableOptions<T>.PostConfigure` 给可空属性兜底默认值。
- `RunOptions` 暴露 `Default` 静态实例 + `Main(args)` 工厂，与 `Serve.Run(RunOptions.Main(args))` 调用约定一致。

## Dependencies
### Internal
- `Poxiao.ConfigurableOptions`
### External
- `Microsoft.AspNetCore.Builder`、`Microsoft.Extensions.Configuration`、`Microsoft.Extensions.Hosting`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
