<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataSync (Dto)

## Purpose
跨库数据同步 DTO。提供执行同步动作的输入与同步结果输出。

## Key Files
| File | Description |
|------|-------------|
| `DbSyncActionsExecuteInput.cs` | 执行同步：源库、目标库、表、增量/全量策略 |
| `DbSyncOutput.cs` | 同步执行结果（成功/失败行数、耗时） |

## For AI Agents

### Working in this directory
- 同步动作可能耗时较长，调用方应使用异步/任务方式；DTO 不承担进度上报，进度走 SignalR/WebSocket。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
