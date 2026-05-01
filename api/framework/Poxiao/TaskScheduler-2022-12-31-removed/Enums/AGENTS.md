<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

> **DEPRECATED / LEGACY**：旧版 TaskScheduler 枚举。新代码请使用 `Poxiao.Schedule` 中的对应枚举。

## Purpose
旧调度器使用的三个公共枚举：任务触发类型、运行状态、执行模式。皆带 `[Obsolete]`、`[SuppressSniffer]` 与中文 `[Description]`。

## Key Files
| File | Description |
|------|-------------|
| `SpareTimeTypes.cs` | `Interval`（间隔方式）/ `Cron`（Cron 表达式）。 |
| `SpareTimeStatus.cs` | `Running` / `Stopped` / `Failed` / `CanceledOrNone` 四种状态。 |
| `SpareTimeExecuteTypes.cs` | `Parallel`（并行）/ `Serial`（串行）执行策略。 |

## For AI Agents

### Working in this directory
- 不要新增枚举值；新功能请到 `Poxiao.Schedule` 中实现。
- 中文 `[Description]` 由前端文档与状态展示直接消费，删除/重命名会破坏外部 UI。

### Common patterns
- `[Obsolete]` + `[SuppressSniffer]` + `[Description("中文")]`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
