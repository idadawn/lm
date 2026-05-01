<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao

## Purpose
检测室数据分析系统的内部框架库（Furion 派生），是整个 .NET 模块化单体的基础。提供应用启动、依赖注入、缓存、授权、动态 API、规范化文档、调度、事件总线、配置选项绑定等横切能力，所有业务模块（`api/src/modularity/**`）都依赖该程序集。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.csproj` | 框架项目文件（.NET 10），声明对 Microsoft.AspNetCore、SqlSugar、CSRedis、Newtonsoft.Json 等核心依赖 |
| `GlobalUsings.cs` | 框架内统一的 `global using`，简化业务侧引用 |
| `Poxiao - Backup.csproj` | 旧版项目文件备份，构建时不参与 |
| `TaskScheduler-2022-12-31-removed/` | 已废弃的任务调度模块（保留作历史参考，新代码用 `Schedule/`） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `App/` | 全局 `App` 静态门面、宿主启动、`AppStartup` 体系 (see `App/AGENTS.md`) |
| `AspNetCore/` | ASP.NET Core 增强（HTTP 扩展、模型绑定器） (see `AspNetCore/AGENTS.md`) |
| `Authorization/` | 策略授权（`[AppAuthorize]`、`AppAuthorizeHandler`） (see `Authorization/AGENTS.md`) |
| `Cache/` | 统一缓存接口（内存 / Redis 实现） (see `Cache/AGENTS.md`) |
| `ClayObject/` | 动态对象 `Clay`（类似 JS object） (see `ClayObject/AGENTS.md`) |
| `Components/` | 组件化注入（`IServiceComponent`、`IApplicationComponent`、`[DependsOn]`） (see `Components/AGENTS.md`) |
| `ConfigurableOptions/` | 配置选项绑定（`IConfigurableOptions<T>`） |
| `DependencyInjection/` | DI 标记接口（`IScoped`/`ITransient`/`ISingleton`）与扫描注册 |
| `DynamicApiController/` | 动态 WebAPI（基于业务服务自动生成 Controller） |
| `Schedule/`、`TaskQueue/` | 周期任务与后台队列 |
| `SpecificationDocument/` | Swagger/规范化文档生成 |

(其他子目录如 `EventBus/`、`Logging/`、`UnifyResult/` 等同样是横切能力，详见各自 `AGENTS.md`。)

## For AI Agents

### Working in this directory
- 这里是**框架层**，不放业务实体或控制器；任何与 LIMS 业务相关的代码都应放到 `api/src/modularity/**`。
- 修改公共 API（`App.*`、扩展方法）时务必兼顾 Web 和 Worker（非 Web）两种宿主，参考 `App/App.cs` 中对 `InternalApp.Configuration == null` 的兜底。
- 新增横切能力优先沿用现有目录布局：`Attributes/Extensions/Handlers/Internal/Options/Providers/Requirements`。

### Common patterns
- `[SuppressSniffer]` 标注于框架内部 API，避免被反射扫描误注册。
- 服务通过实现 `IScoped`/`ITransient`/`ISingleton` 标记接口由 `DependencyInjection` 自动扫描注入。
- 配置类通过 `IConfigurableOptions<T>` + `[Configuration]` 与 `appsettings.json` 节点绑定。

## Dependencies
### Internal
- 被 `api/src/application/Poxiao.API.Entry` 与所有 `api/src/modularity/**` 引用。
### External
- ASP.NET Core / Microsoft.Extensions.* (Hosting, Configuration, DependencyInjection, Logging)
- Newtonsoft.Json、System.Text.Json、CSRedisCore、StackExchange.Profiling、SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
