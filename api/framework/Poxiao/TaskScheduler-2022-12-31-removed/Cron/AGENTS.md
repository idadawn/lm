<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Cron

> **DEPRECATED / LEGACY**：旧版自实现 Cron 解析器。新代码请使用 `Poxiao.TimeCrontab` 模块。

## Purpose
旧 TaskScheduler 内置的 Cron 表达式解析与下次发生时间计算实现，移植自 [Hangfire/Cronos](https://github.com/HangfireIO/Cronos)。提供 5/6 字段两种格式（含/不含秒），位运算优化的 De Bruijn 序列查找，以及时区/日历辅助。

## Key Files
| File | Description |
|------|-------------|
| `CronExpression.cs` | 主解析器（≈900 行）：`Parse(expression, format)`、`GetNextOccurrence(time, zone)`；预定义 `Yearly` / `Weekly` / `Monthly` / `Daily` / `Hourly` / `Minutely` / `Secondly` 静态实例。 |
| `CronField.cs` | 各字段域定义（秒/分/时/日/月/周），含名称缩写表（JAN..DEC, SUN..SAT）与 `AllBits` 位掩码。 |
| `CronFormat.cs` | 格式枚举（Standard / IncludeSeconds）。 |
| `CronExpressionFlag.cs` | 解析中间标志位枚举。 |
| `CronFormatException.cs` | 表达式格式错误异常。 |
| `CalendarHelper.cs` | 月末/闰年/星期 N 等日历计算。 |
| `DateTimeHelper.cs` | DateTime 与 DateTimeKind 处理辅助。 |
| `TimeZoneHelper.cs` | 时区转换辅助（处理 DST 边界）。 |

## For AI Agents

### Working in this directory
- 不要修复 bug——优先迁移调用方到 `Poxiao.TimeCrontab.Crontab`，二者 API 类似但内部实现独立。
- 解析路径使用 `unsafe` 字符指针并依赖 `[MethodImpl(AggressiveInlining)]`，谨慎重构。

### Common patterns
- 位掩码（`long AllBits`）+ De Bruijn 序列高速 NextSetBit 查找。

## Dependencies
### External
- `System.Runtime.CompilerServices`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
