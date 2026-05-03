<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
`App` 静态门面的内部副本——存放运行时可变状态（`InternalServices`、`RootServices`、`Configuration`、`WebHostEnvironment`、`HostEnvironment`），并提供 Web/泛型主机的 `ConfigureApplication` 装配入口与 JSON 配置自动加载（含目录扫描与忽略列表）。

## Key Files
| File | Description |
|------|-------------|
| `InternalApp.cs` | 框架运行时状态容器；`ConfigureApplication(IWebHostBuilder, IHostBuilder)` 自动调用 `ConfigureAppConfiguration`/`ConfigureServices`，加载 `appsettings*.json`、按 `InjectOptions` 扫描目录并合并配置 |

## For AI Agents

### Working in this directory
- 这是 `internal` 实现，**不要从业务模块直接引用**——通过 `App.*` 静态门面访问。
- 修改配置加载顺序时务必维持：基础 `appsettings.json` → 环境特化 → 扫描目录补充 → 命令行/环境变量优先级，与业务侧 `appsettings.dev.json` 等约定保持一致。
- 兼容 .NET 5→6 升级残留的 `IHostBuilder` 参数路径，不要轻易移除。

### Common patterns
- 静态字段 + 静态构造方法：所有运行时状态由 `App.*` 写入此处，只读暴露给框架其他子系统。
- `Microsoft.Extensions.FileSystemGlobbing.Matcher` 用于通配符匹配 JSON 配置文件。

## Dependencies
### Internal
- 被 `App/App.cs`、`App/Filters/StartupFilter.cs`、`App/Startups/HostingStartup.cs`、`App/Extensions/HostBuilderExtensions.cs` 使用。
### External
- `Microsoft.Extensions.Configuration`、`Microsoft.Extensions.Hosting`、`Microsoft.Extensions.FileSystemGlobbing`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
