<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# MessageMonitor

## Purpose
消息发送监控记录（`MessageMonitorEntity`）的查询、列表与批量删除 DTO，用于排查渠道下发失败/重试。

## Key Files
| File | Description |
|------|-------------|
| `MessageMonitorQuery.cs` | 列表查询参数（消息类型 / 来源 / 时间区间 / 接收人） |
| `MessageMonitorListOutput.cs` | 列表行（messageType/messageSource/title/sendTime/receiveUser/content） |
| `MessageMonitorDelInput.cs` | 批量删除主键集合 |

## For AI Agents

### Working in this directory
- 监控数据高频写入，列表查询应分页 + 索引（按 `sendTime`、`messageType`），不要全表扫描。
- `content` 可能很长（含 HTML），列表场景应截断或仅返回摘要。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
