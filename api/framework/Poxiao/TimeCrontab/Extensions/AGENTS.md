<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Extensions

## Purpose
TimeCrontab 内部拓展方法，桥接 .NET `DayOfWeek` 与 Cron 数值表示之间的差异（C# `Sunday=0`，但 Cron 中星期日同时支持 `0/7`），并提供月内最后一个特定星期几的计算辅助。

## Key Files
| File | Description |
|------|-------------|
| `DayOfWeekExtensions.cs` | `ToCronDayOfWeek` / `ToDayOfWeek` 双向转换；`LastDayOfMonth(year, month)` 用于解析 `L`（last weekday）等表达式。 |

## For AI Agents

### Working in this directory
- 类与方法均为 `internal`，不要从模块外部调用。
- 修改星期映射时必须同步检查 `Constants.CronDays` 字典与各 Parser 的取模逻辑（`SpecificParser` 对 `DayOfWeek` 做 `% 7` 兼容）。

### Common patterns
- 通过查 `Constants.CronDays` 字典做映射，避免散落的硬编码。
- `LastDayOfMonth` 采用从月末递减回溯的方式找最后一个目标星期几。

## Dependencies
### Internal
- 依赖同模块内 `Constants.CronDays`。
### External
- 仅 `System.DateTime` / `DayOfWeek`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
