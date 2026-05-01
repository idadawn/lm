<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Message

## Purpose
系统消息（`MessageEntity`）的输入/输出 DTO：草稿/发布、列表、详情、已读、公告以及流程消息（审批/委托）。

## Key Files
| File | Description |
|------|-------------|
| `MessageCrInput.cs` | 创建消息入参 |
| `MessageUpInput.cs` | 更新消息入参 |
| `MessageInfoOutput.cs` | 详情（含正文、附件、过期时间等） |
| `MessageListOutput.cs` | 列表行（id/title/type/releaseUser/releaseTime/isRead/flowType） |
| `MessageListQueryInput.cs` | 列表查询参数 |
| `MessageNoticeOutput.cs` | 通知公告聚合输出（excerpt/category/expirationTime） |
| `MessageNoticeQueryInput.cs` | 通知公告查询参数 |
| `MessageReadInfoOutput.cs` | 已读情况详情 |

## For AI Agents

### Working in this directory
- `type`：1 通知公告，2 系统消息，3 私信。`flowType`：1 审批，2 委托——与 `MessageEntity` 字段一致，新增类型必须同步更新两侧。
- `releaseUser`/`releaseTime` 在 DTO 中，但实体字段是 `CreatorUser` / `CreatorTime`；映射在 Mapster/手写 Select 中完成。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
