<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ImReply

## Purpose
聊天会话列表与对象 ID 输出 DTO。`ImReplyService` (`api/message/imreply`) 通过它聚合最新消息、未读数与对端用户信息。

## Key Files
| File | Description |
|------|-------------|
| `ImReplyListOutput.cs` | 会话列表输出：对端 RealName/HeadIcon、最新消息/时间、未读数、消息类型 |
| `ImReplyObjectIdOutput.cs` | 仅返回对象主键 ID 的轻量输出 |

## For AI Agents

### Working in this directory
- `sendUserId` / `UserId` 标注 `[JsonIgnore]`，仅用于服务端二次匹配，不要在前端依赖。

### Common patterns
- 字段同时混用 PascalCase（如 `RealName`/`UserId`）与 camelCase（如 `sendUserId`），保留与历史前端约定，不要统一。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
