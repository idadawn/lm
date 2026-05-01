<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enums

## Purpose
WebSocket / IM 信令的方法类型枚举，用于前后端协议一致。

## Key Files
| File | Description |
|------|-------------|
| `MessageReceiveType.cs` | 接收方向消息体类型：text / image / voice |
| `MessageSendType.cs` | 发送方向方法：online / initMessage / sendMessage / receiveMessage / messageList / logout / error / closeSocket |
| `MothodType.cs` | 历史/扩展方法类型枚举（注意原文件名沿用拼写 "Mothod"） |

## For AI Agents

### Working in this directory
- 枚举值小写命名（与前端 JSON 字符串一致）；新增成员后，`MessageBase.method` JSON 序列化使用 `JsonStringEnumConverter`，前端无需改动。
- 不要重命名 `MothodType.cs`（拼写沿用，重命名会破坏其他引用）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
