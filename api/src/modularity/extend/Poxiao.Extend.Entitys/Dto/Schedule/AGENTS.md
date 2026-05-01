<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Schedule

## Purpose
日程模块 DTO。提供日程的 CRUD + 列表查询，含微信提醒标记。

## Key Files
| File | Description |
|------|-------------|
| `ScheduleCrInput.cs` / `ScheduleUpInput.cs` | 日程创建/更新（startTime/endTime/content/weChatAlert） |
| `ScheduleListQuery.cs` | 列表查询 |
| `ScheduleListOutput.cs` | 列表项 |
| `ScheduleInfoOutput.cs` | 详情 |

## For AI Agents

### Working in this directory
- `weChatAlert`：1-提醒，0-不提醒；与 `ScheduleService` 的微信发送逻辑保持一致。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
