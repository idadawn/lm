<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
Schedule 模块的内部工具集合。`Penetrates` 集中了序列化、命名转换、SQL 字面量等共享函数；`DynamicJob` 让运行时委托作业可以无类型注册；`Logging.cs` 与 `RepeatKeyEqualityComparer` 提供日志类别和字典 hack。

## Key Files
| File | Description |
|------|-------------|
| `Penetrates.cs` | 静态工具：`GetDefaultJsonSerializerOptions`（带 `DateTimeJsonConverter`）、`Serialize/Deserialize`、`GetNowTime`/`GetUnspecifiedTime`、`SplitToWords`/`GetNaming`（多命名法）、`GetNoNumberSqlValueOrNull`/`GetBooleanSqlValue`（按 `ScheduleOptionsBuilder.InternalBuildSqlType` 适配）、`Write` 高性能 `Utf8JsonWriter`、`GetJsonElementValue` 类型还原、`LoadAssembly`。 |
| `DynamicJob.cs` | `internal sealed`：执行 `JobDetail.DynamicExecuteAsync` 委托，使运行时动态作业不需要单独类型。 |
| `Logging.cs` | `System.Logging.DynamicJob` 类别名占位（用作 `ILogger<DynamicJob>` 的 categoryName）。 |
| `RepeatKeyEqualityComparer.cs` | 允许 `JobDetail` 在字典中"重复键"的比较器（`Equals` 实现为 `x != y`），用于内部去重容差。 |

## For AI Agents

### Working in this directory
- `Penetrates` 默认 JSON 选项被多处共享（看板、SchedulerFactory、HttpJob）；新增 Converter 时需同步评估对外接口契约。
- `GetBooleanSqlValue` / `GetNoNumberSqlValueOrNull` 依赖 `ScheduleOptionsBuilder.InternalBuildSqlType`，新增 SQL 方言要扩展 `SqlTypes` 同时改动这里。
- 不要把 `DynamicJob` 注册成 DI 单例 —— 调度器内部按需创建并通过 `JobDetail.DynamicExecuteAsync` 派发。

### Common patterns
- 命名转换通过 `Regex.Split(@"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})")` 切词，再按 `NamingConventions` 重组。

## Dependencies
### Internal
- `../Constants/SqlTypes`、`../Constants/NamingConventions`、`../Converters/DateTimeJsonConverter`。
### External
- `System.Text.Json`、`System.Text.RegularExpressions`、`System.Reflection`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
