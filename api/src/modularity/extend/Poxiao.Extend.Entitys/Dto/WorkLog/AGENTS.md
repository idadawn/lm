<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# WorkLog

## Purpose
工作日志模块 DTO。提供日志的 CRUD（创建/更新/详情/列表）与抄送分享挂钩。

## Key Files
| File | Description |
|------|-------------|
| `WorkLogCrInput.cs` / `WorkLogUpInput.cs` | 日志创建/更新（含抄送对象） |
| `WorkLogListOutput.cs` | 日志列表 |
| `WorkLogInfoOutput.cs` | 日志详情 |

## For AI Agents

### Working in this directory
- 抄送分享挂在 `WorkLogShareEntity`，DTO 字段（`shareUserIds` 等）需要与服务层 split 写入逻辑一致。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
