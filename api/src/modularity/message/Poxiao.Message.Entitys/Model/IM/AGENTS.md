<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# IM

## Purpose
IM 未读消息聚合 Model，由 `IMContentEntity` 通过 Mapster 映射或 LINQ Group 计算得到。

## Key Files
| File | Description |
|------|-------------|
| `IMUnreadNumModel.cs` | 包含 sendUserId/receiveUserId/unreadNum + defaultMessage/Type/Time，给会话列表与通知红点使用 |

## For AI Agents

### Working in this directory
- `unreadNum` 由 `IMContentEntity.State` 经 Mapster 映射；新增维度（如群聊）需在 Mapper.cs 增加配置。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
