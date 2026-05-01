<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Exceptions

## Purpose
TimeCrontab 模块专用异常类型。当 Cron 表达式解析、字段越界或下一发生值计算失败时，由各 `Parsers/*` 解析器抛出 `TimeCrontabException`，向上层调度器（如 Schedule 任务模块）传递结构化错误。

## Key Files
| File | Description |
|------|-------------|
| `TimeCrontabException.cs` | 密封异常类，标记 `[SuppressSniffer]`，提供消息/内部异常三种构造函数。 |

## For AI Agents

### Working in this directory
- 仅用于 TimeCrontab 解析期错误，不要在业务代码里直接 throw 此异常；业务异常请用 `Poxiao.FriendlyException` 中的 `AppFriendlyException`。
- 类标记 `sealed`，不应继承；新增解析失败语义建议在 message 中区分而非派生子类。

### Common patterns
- 解析器在 `Next()` / `First()` 中对 Day/Month/DayOfWeek 等不可单值递推字段抛出此异常以阻断错误调用。
- 范围/步长越界（`RangeParser`、`StepParser`、`SpecificYearParser`）通过此异常报告字段名与数值。

## Dependencies
### Internal
- 由 `../Parsers/` 下所有 ICronParser 实现使用。
### External
- 仅依赖 `System.Exception`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
