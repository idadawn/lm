<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Internal

## Purpose
TaskQueue 模块内部辅助类型。当前仅含一个用于 `ILogger<>` 分类名的标记类，使日志输出统一以 `TaskQueueService` 作为分类，便于按模块过滤。

## Key Files
| File | Description |
|------|-------------|
| `Logging.cs` | `internal sealed class TaskQueueService` 空标记类，位于 `System.Logging` 命名空间，专供 `ILogger<TaskQueueService>` 注入使用。 |

## For AI Agents

### Working in this directory
- 不要把这里当成业务逻辑入口——它只是个分类锚点。
- 新增内部辅助类型保持 `internal sealed`，并尽量放在同一命名空间下以便按目录归类。

### Common patterns
- 使用空类作为 `ILogger<T>` 的 T，避免暴露内部实现类型给业务侧。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
