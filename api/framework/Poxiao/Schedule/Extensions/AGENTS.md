<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
Schedule 模块的入口拓展。包含 DI 注册（`AddSchedule`）与运行时辅助（类型扫描、属性映射、时间格式化），把作业类型自动转换为 `SchedulerBuilder`/`TriggerBuilder` 并注入后台主机服务。

## Key Files
| File | Description |
|------|-------------|
| `ScheduleServiceCollectionExtensions.cs` | `AddSchedule(...)` 重载：构建 `ScheduleOptionsBuilder`，注册 `IScheduleLogger`、`ISchedulerFactory` 单例与 `ScheduleHostedService` 后台服务，订阅 `UnobservedTaskException`。 |
| `ScheduleExtensions.cs` | 扫描 `IJob` 类型生成 `SchedulerBuilder`/`TriggerBuilder`（识别 `[JobDetail]`/`[Trigger]`），提供 `GetScheduleHostedService`、`MapTo<T>`（多命名法属性映射）、`ToUnspecifiedString`、`GetMaxLengthString` 等工具。 |

## For AI Agents

### Working in this directory
- `AddSchedule` 必须传入 `ScheduleOptionsBuilder`（直接或委托）；选项内含 `UseUtcTimestamp`、`ClusterId`、`LogEnabled`、`UnobservedTaskExceptionHandler`。
- `ScanToBuilders` 默认跳过非 `IJob` 的类型；扩展扫描时复用 `IsJobType()`，避免接口/抽象类被错误注册。
- `MapTo<T>` 支持 PascalCase / camelCase / under_score_case 同时存在的字典源——保持该容错以兼容 SqlSugar 字段命名差异。

### Common patterns
- 扩展类放在 `Microsoft.Extensions.DependencyInjection` / `Microsoft.AspNetCore.Builder` 命名空间下，保持调用 `services.AddSchedule(...)` 不需额外 `using`。

## Dependencies
### Internal
- `../HostedServices/ScheduleHostedService`、`../Loggers/`、`../Factories/SchedulerFactory`、`../Triggers/`。
### External
- `Microsoft.Extensions.{DependencyInjection,Hosting,Logging}`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
