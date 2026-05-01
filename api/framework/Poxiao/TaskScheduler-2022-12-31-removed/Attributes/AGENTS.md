<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

> **DEPRECATED / LEGACY**：旧版 TaskScheduler 的特性。新代码请使用 `Poxiao.Schedule` 的相应特性。

## Purpose
旧版 `[SpareTime]` 定时任务特性，标注在 `ISpareTimeWorker` 实现类的方法上，支持 Interval（毫秒）或 Cron 表达式两种触发方式。

## Key Files
| File | Description |
|------|-------------|
| `SpareTimeAttribute.cs` | `AttributeUsage(Method, AllowMultiple=true)` + `[Obsolete]`。两种构造：`(double interval, workerName?)` 与 `(string expressionOrKey, workName?)`——后者通过 `expressionOrKey.Render()` 支持配置文件占位；若可解析为 `long` 则视为 Interval，否则为 Cron。还包含 `DoOnce` / `StartNow` / `ExecuteType` (`Parallel`) / `CronFormat` 等开关。 |

## For AI Agents

### Working in this directory
- 不要新增字段；如需扩展请在 `Poxiao.Schedule` 中实现。
- Cron 解析依赖父目录的 `Cron/CronExpression.cs`，与 `Poxiao.TimeCrontab.Crontab` 不同源。

### Common patterns
- 使用 `Render()` 让字符串参数支持 `#(Config:Path)` 配置占位。

## Dependencies
### Internal
- `Poxiao.Templates.Extensions.Render`、本目录父级的 `SpareTimeTypes` / `SpareTimeExecuteTypes`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
