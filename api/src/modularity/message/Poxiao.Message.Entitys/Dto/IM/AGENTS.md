<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# IM

## Purpose
WebSocket IM 即时通讯的传输层 DTO：消息基类、文字/图片/语音入参、聊天记录列表输出与在线用户输出。

## Key Files
| File | Description |
|------|-------------|
| `MessageBase.cs` | 基类，含 `MessageSendType method`（`JsonStringEnumConverter`），所有 WebSocket 消息派生于此 |
| `MessageInput.cs` | 文本消息入参 |
| `MessagetImageInput.cs` | 图片消息入参（注意拼写沿用：`MessagetImageInput`） |
| `MessageVoiceInput.cs` | 语音消息入参 |
| `IMContentListOutput.cs` | 历史记录列表输出（id / send/receive 用户 / 时间 / 内容 / 状态） |
| `OnlineUserListOutput.cs` | 在线用户列表（userId / account / userName / loginTime / IP / 平台） |

## For AI Agents

### Working in this directory
- DTO 必须配合 `Poxiao.Message.Entitys.Enums.MessageSendType` / `MessageReceiveType` 使用，新增方法类型先扩枚举。
- 与 `Mapper.cs` 中的 `UserOnlineModel → OnlineUserListOutput` 映射保持字段顺序对应。

### Common patterns
- 字段全部 camelCase；属性无注解（DI 容器中传输用，不直接进 SqlSugar）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
