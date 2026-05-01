<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Parsers

## Purpose
TimeCrontab Cron 表达式字段解析器集合。每个文件对应 Cron 中一种字符或语法（`*`、具体值、`-` 范围、`/` 步长、`L` 系列、`W` 工作日、`#` 月内第 N 个星期几等），实现 `ICronParser` / `ITimeParser` 用于判断匹配并计算字段下一发生值。

## Key Files
| File | Description |
|------|-------------|
| `AnyParser.cs` | `*` 任意值解析器；秒/分/时/年支持 `Next/First`，天/月/周抛 TimeCrontabException。 |
| `SpecificParser.cs` | 具体数值解析器；DayOfWeek 做 `% 7` 兼容，并校验上下界。 |
| `RangeParser.cs` | `start-end[/steps]` 范围+步长，预生成 SpecificParser 列表。 |
| `StepParser.cs` | `start/steps` 间隔解析器；构造期枚举所有匹配值。 |
| `LastDayOfMonthParser.cs` | `L` — 当月最后一天。 |
| `LastDayOfWeekInMonthParser.cs` | `5L` — 当月最后一个特定星期几。 |
| `LastWeekdayOfMonthParser.cs` | `LW` — 当月最后一个工作日。 |
| `NearestWeekdayParser.cs` | `15W` — 离指定日最近的工作日。 |
| `SpecificDayOfWeekInMonthParser.cs` | `5#3` — 当月第 N 个特定星期几。 |
| `BlankDayOfMonthOrWeekParser.cs` | `?` — Day/DayOfWeek 字段忽略。 |
| `SpecificYearParser.cs` | 年字段具体值解析。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dependencies/` | ICronParser / ITimeParser 接口定义 (see `Dependencies/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 所有解析器类均为 `internal sealed`，被 `Crontab.Parse` 流程内部装配，不要从模块外直接 new。
- 实现新解析器时遵循三件套：构造期校验、`IsMatch(DateTime)` 经 `Kind` switch 取值、复杂字段（Day/Month/DayOfWeek）的 `Next/First` 抛 `TimeCrontabException`（高层在 Crontab 上做组合推算）。
- `ToString()` 必须可逆——它用于把 `Crontab` 对象格式化回 Cron 字符串。

### Common patterns
- 共用 `Constants.MinimumDateTimeValues` / `MaximumDateTimeValues[Kind]` 做边界校验。
- 范围/步长类解析器在构造时预先展开为 `IEnumerable<SpecificParser>`，避免每次匹配重复计算。

## Dependencies
### Internal
- `../Exceptions/TimeCrontabException`、`../Extensions/DayOfWeekExtensions`、模块内 `Constants` & `CrontabFieldKind`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
