<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dashboard

## Purpose
工作台首页（`DashboardService`）的输出 DTO：待办、邮件、通知、流程任务统计等。

## Key Files
| File | Description |
|------|-------------|
| `FlowTodoOutput.cs` | 待办流程任务（id/fullName/creatorTime） |
| `FlowTodoCountOutput.cs` | 待办分桶计数输出 |
| `FlowTodoCountInput.cs` | 待办计数入参（按用户/分类筛选） |
| `EmailOutput.cs` | 邮件预览（标题/发件人/时间） |
| `NoticeOutput.cs` | 系统通知/公告输出 |

## For AI Agents

### Working in this directory
- 这些 DTO 由 `DashboardService` 聚合 `EmailReceiveEntity` / `IFlowTaskRepository` / `IMessageService` 数据后返回；接口路径 `api/visualdev/Dashboard`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
