<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Message.Interfaces

## Purpose
消息模块对外契约工程：抽象出可被其它模块（VisualDev、Lab、System、Workflow）注入的服务与协调器接口，避免互相依赖实现。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Message.Interfaces.csproj` | 工程文件（仅引用 `Poxiao.Common.Core` + `Systems.Interfaces` + `WorkFlow.Entitys`） |
| `IMessageService.cs` | 系统消息发送：`SentMessage(toUserIds, title, body, dict, type, flowType)` |
| `IMessageManager.cs` | 协调器：`SendDefaultMsg`、`SendDefinedMsg`、`GetMessageEntity`、`GetMessageReceiveList`、`GetMessageSendModels`、`ForcedOffline` |
| `IImReplyService.cs` | 聊天会话查询接口 |
| `IShortLinkService.cs` | 短链生成/跳转接口 |

## For AI Agents

### Working in this directory
- 接口必须**只引用** Entitys 和 Interfaces 项目；任何业务实现细节不得泄漏到这里。
- `IMessageService` 是其它模块通知用户最常用的入口；新增高级用法（如延迟发送、回执）请扩展 `IMessageManager` 而非 `IMessageService`。

### Common patterns
- 命名空间 `Poxiao.Message.Interfaces`（`IMessageService` 在子命名空间 `Poxiao.Message.Interfaces.Message`）。

## Dependencies
### Internal
- `Poxiao.Message.Entitys`、`Poxiao.Common.Core`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
