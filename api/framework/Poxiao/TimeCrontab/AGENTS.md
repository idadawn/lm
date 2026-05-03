<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# TimeCrontab

## Purpose
现代 Cron 表达式 OOP 抽象。`Crontab.Parse(expr, format)` 把 5/6/7 字段表达式转为对象，支持 `@secondly`/`@minutely`/`@hourly`/`@daily`/`@monthly`/`@weekly`/`@yearly`/`@workday` 宏与 `@xxxAt(fields)` 偏移构造，提供 `GetNextOccurrence(s)` / `GetSleepMilliseconds` / `GetSleepTimeSpan`。被 `Poxiao.TaskQueue` 与 `Poxiao.Schedule` 共同依赖。

## Key Files
| File | Description |
|------|-------------|
| `Crontab.cs` | 公共入口（`partial`）：`Parse` / `TryParse` / `IsValid` / `GetNextOccurrence(s)` / `GetSleepMilliseconds` / `ToString`；处理 `@xxx` 宏路由。 |
| `Crontab.Internal.cs` | 内部解析与下次时间推算（≈700 行）：字段解析、 `InternalGetNextOccurence`、按 `CrontabFieldKind` 拼接输出。 |
| `Crontab.Macro.cs` | 七个 Macro 静态实例（Yearly/Weekly/...）。 |
| `Crontab.MacroAt.cs` | `SecondlyAt(...) / MinutelyAt(...) / ...` 带具体字段值的 Macro 工厂。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Constants/` | 字段最值/枚举/格式常量 (see `Constants/AGENTS.md`) |
| `Exceptions/` | `TimeCrontabException`（解析失败抛出） |
| `Extensions/` | `DayOfWeekExtensions`（C# DayOfWeek <-> Cron 0-7 映射） |
| `Parsers/` | `*Parser.cs`（Any/Specific/Range/Step/LastDay/NearestWeekday 等 Cron 字段解析器） |

## For AI Agents

### Working in this directory
- 与同仓的 `TaskScheduler-2022-12-31-removed/Cron` 是**两套独立**的 Cron 实现；新代码统一使用本模块。
- 默认 `CronStringFormat.Default` 为 5 字段（分时日月周）；秒/年字段需显式选 `WithSeconds` / `WithYears` / `WithSecondsAndYears`，否则解析报 `TimeCrontabException`。
- `Parsers/Dependencies/` 内是接口与基类，新增字段语法（如 `W`、`L`、`#`）请新增对应 `*Parser.cs`，并在 `Crontab.Internal.cs` 的字段拼装逻辑中登记。

### Common patterns
- `partial class Crontab` 拆 4 个文件按职责分离；解析器实现 `ICronParser` 接口；`CrontabFieldKind` 字典作为字段表。

## Dependencies
### Internal
- 自包含模块；被 `Poxiao.TaskQueue` / `Poxiao.Schedule` 引用。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
